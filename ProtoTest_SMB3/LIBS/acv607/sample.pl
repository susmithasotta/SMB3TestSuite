#!/usr/local/bin/perl
# Execute all sample programs in given subdirectory.  Note that this script 
# can also be used to execute scripts in a test directory be using the 
# test subdir name in place of encoding rules (for example, test_xml)..

use File::Spec;
use Cwd;

# for the duration of the script, we want to have the local directory included
# in the $PATH
if ($^O =~ /Win/) {
   $ENV{"PATH"} = $ENV{"PATH"} . ';.;..\..\..\bin';
}
else { 
   $ENV{"PATH"} = $ENV{"PATH"} . ':.:../../../bin';
}

select STDERR; $| = 1;
select STDOUT; $| = 1;

sub usage {
    print "usage: sample.pl <subdir> <rules> <function>\n";
    print "  <subdir>       language subdirectory\n";
    print "                 (c, cpp, java, cpp_borland, csharp)\n";
    print "  <rules>        encoding rules or test subdirectory\n";
    print "                 (ber, der, per, xer, xml, test_*, all)\n";
    print "  <function>     function (test or clean)\n";
    print "  [-nodir]       execute <function> in subdir (nodir)\n";
    print "  [-make <make>] name of make utility to invoke (ex. nmake)\n";
    print "  [-stop]        quit on error\n";
    exit(1);
}

usage() if ($#ARGV < 2);

$subdir = shift @ARGV;
$rules  = shift @ARGV;
$function = shift @ARGV;
my $nostop = 1;
my $nerror = 0;
my $hread = 0;
my $makeprog;
while ($_ = shift @ARGV) {
    if ($_ eq '-make') {
        usage() unless $makeprog = shift @ARGV;
        next;
    } elsif ($_ eq '-stop') {
        $nostop = 0;
    } elsif ($_ eq '-nodir') {
        $nodir = 1;
    } elsif ($_ eq '-hread') {
        $hread = 1;
    } else {
        print STDERR "Unknown option '$_'\n";
        usage();
    }
}

if($verbose) { $hread = 1; }

die ("$subdir is not a known subdirectory " .
     "(must be c, cpp, cpp_borland, csharp or java)") 
    unless ($subdir eq "c" || $subdir eq "cpp" || 
            $subdir eq "cpp_borland" || $subdir eq "java" 
            || $subdir eq "csharp");

die ("$rules are not valid encoding rules " . 
     "(must be ber, cer, der, per, xer, xml, test_*, or all)") 
    unless ($rules eq "ber" || $rules eq "cer" || $rules eq "der" || 
            $rules eq "per" || $rules eq "xer" || $rules eq "xml" || 
            $rules eq "all" || $rules =~ /^test_/ || $nodir);

die ("$function is not a known function (must be test or clean)") 
    unless ($function eq "test" || $function eq "clean");

$makeprog = getMakeProg() unless $makeprog;

# try to determine if Windows or UNIX platform type
$XSDLIBDIR = "../../../xsd/lib";
$XERCESDIR = "../../../xsd/xmlParsers/xerces-2_2_0";
$XERCESCLASSPATH = "\".;$XERCESDIR/xercesImpl.jar;$XERCESDIR/xmlParserAPIs.jar;$XERCESDIR/xercesSamples.jar\"";

chdir ("./$subdir");

if ($rules eq "all") {
    print "--- Executing $function on $subdir ber samples ---\n";
    &execSamples ("sample_ber", $function);
    print "--- Executing $function on $subdir der samples ---\n";
    &execSamples ("sample_der", $function);
    print "--- Executing $function on $subdir per samples ---\n";
    &execSamples ("sample_per", $function);
    print "--- Executing $function on $subdir xer samples ---\n";
    &execSamples ("sample_xer", $function);
    print "--- Executing $function on $subdir xml samples ---\n";
    &execSamples ("sample_xml", $function);
}
elsif ($nodir) {
    chdir($rules) or die "Unable to change directory: $!";
    execSampleInCwd($subdir, ".", $rules, $function);
    chdir("..");
}
elsif ($rules =~ /test_/) {
    &execSamples ($rules, $function);
}
else {
    print "--- Executing $function on $rules samples ---\n";
    &execSamples ("sample_${rules}", $function);
}
exit $nerror; 

sub execSampleInCwd {

    my $subdir = shift @_;
    my $dirName = shift @_;
    my $filename = shift @_;
    my $function = shift @_;

    if ( ! -d "../$filename" ) {
        die "Invalid filename '$filename'";
    }

    # Is this directory in the skip list?
    if (checkSkipList()) {
        printTestName($filename);
        print "SKIPPED\n";
        return;
    }

    # if special test script exists, execute it
    if (-e "test.pl" && $function eq "test") {
        printTestName($filename);
        execCmd("perl test.pl") or return;
        print "\n";
    }
    elsif (-e "clean.pl" && $function eq "clean") {
        execCmd("perl clean.pl") or return;
    }

    # else execute standard test procedure

    elsif (-e "makefile") {
        if ($function eq "test") {
            # major kludge: if java and jsources.mk does not 
            # exist, create a dummy file
            if ($subdir eq "java" && ! -e "jsources.mk") {
                &genJsources ();
                #open (MKFILE, ">jsources.mk");
                #print MKFILE "JSOURCES = \n";
                #close (MKFILE);
            }
            if ($subdir eq "csharp") {
                `cp ../../asn1rt.dll .`;
            }

            printTestName($filename);
            execCmd($makeprog) or return;

            if ($dirName =~ /_ber/) {
                &doBERTest ($subdir, 0);
            }
            elsif ($dirName =~ /_der/) {
                &doBERTest ($subdir, 1);
            }
            elsif ($dirName =~ /_cer/) {
                &doBERTest ($subdir, 1);
            }
            elsif ($dirName =~ /_per/) {
                &doPERTest ($subdir);
            }
            elsif ($dirName =~ /_xer/) {
                &doXERTest ($subdir);
            }
            elsif ($dirName =~ /_xml/) {
                &doXMLTest ($subdir);
            }
            elsif ($dirName =~ /_depends/) {
                &doBERTest ($subdir);
            }
            else {
                die ("unknown subdirectory.");
            }

            print "\n";
        }
        elsif ($function eq "clean") {
            execCmd("$makeprog clean") or return;
        }
    }
}

sub execSamples {
    local ($dirName) = @_;
    return if (!(-e $dirName));

    chdir ($dirName);
    @dirlist = (`ls .`);

    foreach $filename (@dirlist) {
        $filename =~ s/\n$//;

        if (-d $filename) {
            next if ($filename eq "employee_dll");
            next if ($filename eq "EmployeeSocketStream");

            chdir ($filename);

            execSampleInCwd($subdir, $dirName, $filename, $function);

            chdir ('..');
        }
    }

    chdir ('..');
    print "\n" if $function eq "clean";
}

sub doBERTest {
    local ($subdir, $derflag) = @_;
    my $out;
    my $ntest = 0;

    if ($subdir eq "java" || $subdir eq "csharp" ) {
        if (-e "writer.log" && -e "writer.glg") {
            execDiff("diff -w writer.log writer.glg") or return;
            $ntest++;
        }
        if (-e "reader.log" && -e "reader.glg") {
            execDiff("diff -w reader.log reader.glg") or return;
            $ntest++;
        }
        if (-e "readeri.log") {
            if (-e "readeri.glg") {
                execDiff("diff readeri.log readeri.glg") or return;
                $ntest++;
            }
            elsif (-e "reader.glg") {
                execDiff("diff readeri.log reader.glg") or return;
                $ntest++;
            }
        }
    }
    else {

        if ((-e "server.exe" || -e "server") && 
            (-e "client.exe" || -e "client")) {
            execClientServer() or return;
            $ntest++;
        }
        if (-e "writer.exe" || -e "writer") {
            execCmd("writer -notrace") or return;
            $ntest++;
        }

        if (-e "reader.exe" || -e "reader") {
            execCmd("reader -notrace") or return;
            $ntest++;
        }

        if (-e "message_ber.dat") {
            execDiff("diff message.dat message_ber.dat") or return;
            $ntest++;
        }

        # convert BER data file to indef length and verify we 
        # can read and decode it

        if ((-e "reader.exe" || -e "reader") 
            && -e "message.dat" 
            && !$derflag) 
        {
            execCmd("ber2indef message.dat messagei.dat") or return;
            execCmd("reader -i messagei.dat -notrace") or return;
            $ntest++;
        }

    }
    print "!" unless $ntest;
}

sub doPERTest {
    local ($subdir) = @_;
    my $out;
    my $ntest = 0;

    if ($subdir eq "java" || $subdir eq "csharp" ) {
        if (-e "writer_a.log" && -e "writer_a.glg") {
            execDiff("diff -w writer_a.log writer_a.glg") or return;
            $ntest++;
        }
        if (-e "reader_a.log" && -e "reader_a.glg") {
            execDiff("diff -w reader_a.log reader_a.glg") or return;
            $ntest++;
        }
        if (-e "writer_u.log" && -e "writer_u.glg") {
            execDiff("diff -w writer_u.log writer_u.glg") or return;
            $ntest++;
        }
        if (-e "reader_u.log" && -e "reader_u.glg") {
            execDiff("diff -w reader_u.log reader_u.glg") or return;
            $ntest++;
        }
    }
    else {
        if ((-e "server.exe" || -e "server") && 
            (-e "client.exe" || -e "client")) {
            execClientServer() or return;
            $ntest++;
        }

        if (-e "writer.exe" || -e "writer") {
            execCmd("writer -a -notrace") or return;
            $ntest++;
        }

        if (-e "reader.exe" || -e "reader") {
            execCmd("reader -a -notrace") or return;
            $ntest++;
        }

        if (-e "message_a.dat") {
            execDiff("diff message.dat message_a.dat") or return;
            $ntest++;
        }

        if (-e "writer.exe" || -e "writer") {
            execCmd("writer -u -notrace") or return;
            $ntest++;

            # only do unaligned reader test if writer exists (this is 
            # because megaco only contains an aligned data file)..

            if (-e "reader.exe" || -e "reader") {
                execCmd("reader -u -notrace") or return;
            }

            if (-e "message_u.dat") {
                execDiff("diff message.dat message_u.dat") or return;
            }
        }
    }
    print "!" unless $ntest;
}

sub doXERTest {
    local ($subdir) = @_;
    my $out;
    my $ntest = 0;

    if ($subdir eq "java" || $subdir eq "csharp" ) {
        if (-e "writer.log" && -e "writer.glg") {
            execDiff("diff -w writer.log writer.glg") or return;
            $ntest++;
        }
        if (-e "reader.log" && -e "reader.glg") {
            execDiff("diff -w reader.log reader.glg") or return;
            $ntest++;
        }
        if (-e "./good/message.xml") {
            execDiff("diff -w message.xml ./good") or return;
            $ntest++;
        }
        if (-e "message.xml" && -e "message.glg") {
            execDiff("diff -w message.xml message.glg") or return;
            $ntest++;
        }
    }
    else {
        if ((-e "server.exe" || -e "server") && 
            (-e "client.exe" || -e "client")) {
            execClientServer() or return;
            $ntest++;
        }

        if (-e "writer.exe" || -e "writer") {
            execCmd("writer -notrace") or return;
            $ntest++;
        }

        if (-e "reader.exe" || -e "reader") {
            execCmd("reader -notrace") or return;
            $ntest++;
        }

        if (-e "./good/message.xml") {
            execDiff("diff -w message.xml ./good/message.xml") or return;
            $ntest++;
        }
    }
    print "!" unless $ntest;
}


sub doXMLTest {
    local ($subdir) = @_;
    my $out;
    my $ntest = 0;

    if ($subdir eq "java" || $subdir eq "csharp" ) {
        if (-e "writer.log" && -e "writer.glg") {
            execDiff("diff -w writer.log writer.glg");
            $ntest++;
        }
        if (-e "reader.log" && -e "reader.glg") {
            execDiff("diff -w reader.log reader.glg");
            $ntest++;
        }
        if (-e "message.xml" && -e "message.glg") {
            execDiff("diff -w message.xml message.glg");
            $ntest++;
        }
    }
    else {
        
        if ((-e "server.exe" || -e "server") && 
            (-e "client.exe" || -e "client")) {
            execClientServer();
            $ntest++;
        }

        if (-e "writer.exe" || -e "writer") {
            execCmd("writer -notrace");
            $ntest++;
        }

        if (-e "reader.exe" || -e "reader") {
            execCmd("reader -notrace");
            $ntest++;
        }
    }

    # this needs to be configurable - should only be done if XERCES exists 
    # (ED, 11/7/2003)
    # if (!(-e "osxsdlib.xsd")) {
    #    `cp -f $XSDLIBDIR/osxsdlib.xsd ./`;
    # }
    # print ("Validating message ..\n");
    # `java -classpath $XERCESCLASSPATH sax.Counter -v -s -f message.xml`;

    if (-e "message.glg") {
        execDiff("diff -w message.xml message.glg");
        $ntest++;
    }
    print "!" unless $ntest;
}

sub genJsources {
    $cmdLine = "";
    $asn1c = "";
    $tempDir = "__!!!__";
    open (INMAKE, "makefile");
    while (<INMAKE>) {
        if (/^\s*(\$\(ASN1C\))/ || /^\s*(\$\(ASN1C90\))/) {
            $asn1c = $1;
            s/^\s*//;
            $cmdLine = $_;
            last;
        }
    }
    close (INMAKE);
    return if ($asn1c eq "" || $cmdLine eq "");

    #print "Cmdline from MAKEFILE: $cmdLine";
    #print "\$ASN1C macro: $asn1c\n";
    $pwd = `pwd`;
    
    $pwd =~ /java/;
    if ($asn1c eq "\$\(ASN1C\)") {
        $asn1cPath = $`."bin/asn1c";
        $cmdLine =~ s/\$\(ASN1C\)/$asn1cPath/;
    }
    else {
        $asn1cPath = $`."bin/asn1c90";
        $cmdLine =~ s/\$\(ASN1C90\)/$asn1cPath/;
    }

    $cmdLine =~ s/\n$//;
    $cmdLine .= " -genjsources -o $tempDir";

    `mkdir $tempDir`;

    $cmdLine =~ s/^\/cygdrive\/(\w)/$1:/;
    #print "$cmdLine\n";

    #`/asn1c/dev/bin/asn1c simple-ROSE.asn -ber -java -print -pkgpfx sample_ber -genjsources -o __\$\$\$\$___`;
    `$cmdLine 2>&1`;

    @files = `ls $tempDir`;
    foreach $file (@files) {
        #print $file;
        $file =~ s/\n$//;
        if ($file =~ /.mk$/) {
            # copy $file to ./jsources.mk and replace $tempDir by .

            open (INSRCMK, "$tempDir/$file");
            open (OUTSRCMK, ">jsources.mk");

            #`cp -f $tempDir/$file ./jsources.mk`;
            while (<INSRCMK>) {
                #print $_;
                s/$tempDir/\./;
                print OUTSRCMK $_;
            }

            close (OUTSRCMK);
            close (INSRCMK);
            last;                    
        }
    }
    `rm -f $tempDir/*`;
    `rmdir $tempDir`;
}

BEGIN {

    my $loadSkipList = sub {
        my $sampledir = getcwd();
        my $skipfile = "$sampledir/skiplist.inc";
        my @list;
        if (-e $skipfile) {
            open SKIP, $skipfile or die "Unable to open '$skipfile': $!";
            foreach (<SKIP>) {
                chomp;
                next if /^\s*$/;
                next if /^#/;
                    @list[++$#list] = $_;
            }
            close SKIP;
        }
        return @list;
    };
    my @skip = &$loadSkipList();

    sub checkSkipList {
        # Is this directory in the skip list?
        $cwd = getcwd();
        foreach $dir (@skip) {
            my $l = length($dir);
            next if $l == 0; 
            if (length($cwd)>$l) {
                if (substr($cwd, -$l, $l) eq $dir) {
                    return 1;
                }
            }
        }
        return 0;
    }
}

sub getMakeProg {

    my $makeprog = "make";
    if (defined ($ENV{'MAKEPROG'})) {
        $makeprog = $ENV{'MAKEPROG'};
    }
    elsif (defined ($ENV{'MSVCDIR'}) || 
           defined ($ENV{'MSDEVDIR'}) ||
           defined ($ENV{'MSDevDir'})) {
        $makeprog = "nmake -nologo";
    }
    else {
        $makeprog = "make";
    }
    return $makeprog;
}

sub printTestName {
    if($hread) { printf "*********************************************************\n"; }
    local ($name) = shift @_;
    printf("$name%" . (30 - length($name)) . "s", '');
}

sub error {
    my $cmd = shift @_;
    my $sig = shift @_;
    my $cwd;
    chomp($cwd = `pwd`);
    
    $nerror++;

    if ($sig == 2) {
        print "\n\nReceived SIGINT, stoping there.\n\n";
        exit 1;
    } elsif ($nostop) {
        if ($function eq "clean") {
            print "\nDirectory: $cwd\n";
        }
        print " ERROR cmd=$cmd\n";
    } else {
        print "\n\n### TEST FAILURE ###\n";
        print "Command: $cmd\n";
        print "Directory: $cwd\n\n";
        exit 1;
    }
}

sub execCmd {
    local ($cmd) = @_;
    my $devnull = File::Spec->devnull();

    if ($hread) {
        print "\n$cmd ";
    } else {
        print (".");
    }
    $cmd =~ tr|/|\\| if ($ENV{'OS'} =~ /Windows/i && 
                         !$ENV{'TERM'} =~ /cygwin/);
    my $_cmd = "$cmd > $devnull";
    $_cmd .= " 2> $devnull" if  $nostop;
    system($_cmd);
    $rc = $?;
    if ($rc != 0) {
        if($hread) { printf "FAIL\n"; }
        error($cmd);
        return 0;
    }
    if($hread) { printf "OK\n"; }
    return 1;
}

sub execDiff {
    local ($cmd) = @_;
    my $out;
    
    if ($hread) {
        print "\n$cmd ";
    } else {
        print (".");
    }
    $out = `$cmd`;
    if (length($out) > 0) {
        if($hread) { printf "FAIL\n"; }
        error($cmd);
        return 0;
    }
    if($hread) { printf "OK\n"; }
}

sub execClientServer {
    
    my $child_pid;
    my $server_rc;
    my $client_rc;
    my $server_cmd = "server";
    my $client_cmd = "client";
    if ($ENV{'OS'} =~ /Windows/i && !$ENV{'TERM'} =~ /cygwin/) {
        $client_cmd =~ tr|/|\\|; 
        $server_cmd =~ tr|/|\\|;
    }
    my $cmd = "server & client";
    if ($verbose) {
        print "\nCommand: $cmd\n";
    } else {
        print (".");
    }
    
    if (!defined($child_pid = fork())) {
        die "cannot fork: $!";
        
    } elsif ($child_pid) {
        
        # start server
        `$server_cmd`;
        $server_rc = $?;
        # wait for child to return
        waitpid($child_pid, 0);
        $client_rc = $?;
        
    } else {
        
        sleep 1; # give some time to the server
        # start the client
        `$client_cmd`;
        # exit with appropriate return code
        exit ($? != 0);
    } 
    
    if (($server_rc != 0) || ($client_rc != 0)) {
        error($cmd);
        return 0;
    }
    return 1;

}

# Same as the system command but the outputs of the command are suppressed
sub silent_system {
    
    my $cmd = shift;
    my $child_pid;
    
    # Work-around older File::Spec modules without devnull method.
    my $devnull = "";
    eval "use File::Spec;";
    eval { $devnull = File::Spec->devnull(); } unless $@;
    $devnull = "/dev/null" if $@;

    if ($ENV{'OS'} =~ /Windows/i) {
        # Fallback to shell redirection for windows
        my $_cmd = "$cmd > $devnull";
        $_cmd .= " 2> $devnull" if  $nostop;
        system($_cmd);
        return;
    }
    
    if (!defined($child_pid = fork()))
    {
        die "cannot fork: $!";
    }
    elsif ($child_pid)
    {
        # wait for child to return
        waitpid($child_pid, 0);
    }
    else
    {
        # silence command output 
        open STDOUT, ">$devnull" or die "Unable to open $devnull: $!";
        open STDERR, ">$devnull" or die "Unable to open $devnull: $!";
        # execute command
        exec($cmd) or die "Cannot execute command."
    } 
    
}
