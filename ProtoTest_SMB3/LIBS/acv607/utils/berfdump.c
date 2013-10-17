/*
 * Copyright (C) 1997-2006 by Objective Systems, Inc.
 *
 * This software is furnished under a license and may be used and copied
 * only in accordance with the terms of such license and with the
 * inclusion of the above copyright notice. This software or any other
 * copies thereof may not be provided or otherwise made available to any
 * other person. No title to and ownership of the software is hereby
 * transferred.
 *
 * The information in this software is subject to change without notice
 * and should not be construed as a commitment by Objective Systems, Inc.
 *
 * PROPRIETARY NOTICE
 *
 * This software is an unpublished work subject to a confidentiality agreement
 * and is protected by copyright and trade secret law.  Unauthorized copying,
 * redistribution or other use of this work is prohibited.
 *
 * The above notice of copyright on this source code product does not indicate
 * any actual or intended publication of such source code.
 *
 *****************************************************************************/

/* Dump the contents of a BER-encoded ASN.1 data file to stdout */

#include "asn1ber.h"
#include <stdio.h>
#include <stdlib.h>
#include <string.h>

#ifndef MIN
#define MIN(a,b) (((a)<(b))?(a):(b))
#endif

#define NUM_SEGMENT_BYTES  12	/* # of bytes to display in a segment */

static char* fmt_contents (FILE* fp, int len, int *count);

int main (int argc, char** argv)
{
   FILE*        fp;
   OSOCTET      msgbuf[16];
   ASN1TAG	tag;
   int		count, len, bufidx, stat;
   char		*bufp, class_text[5], form_text, id_text[5];


   if (argc != 2) {
      printf ("usage: berfdump <filename>\n");
      printf ("  <filename>  Name of file containing BER encoded data\n");
      return 0;
   }

   if ((fp = fopen (argv[1], "rb")) == 0) {
      perror ("fopen");
      printf ("filename: '%s'\n", argv[1]);
      return -1;
   }

   printf 
      ("CLAS  F  -ID-  LENGTH  HEX CONTENTS                         ASCII\n");

   for (;;) {
      bufidx = 0;

      stat = xdf_TagAndLen (fp, &tag, &len, msgbuf, &bufidx);
      if (stat != 0) break;

      xu_fmt_tag (&tag, class_text, &form_text, id_text);

      if ((tag & TM_CONS) || len == 0) {
         if (len == ASN_K_INDEFLEN)
            printf 
               ("%4s  %c  %4s   INDEF\n", class_text, form_text, id_text);
         else
            printf 
               ("%4s  %c  %4s  %6d\n", class_text, form_text, id_text, len);
      }
      else
      {
         int i, offset = 0;

         bufp = fmt_contents (fp, len, &count);

         for (i = 0; i < count; i++)
         {
            if (i == 0)
               printf ("%4s  %c  %4s  %6d  %s\n", 
                  class_text, form_text, id_text, len, &bufp[offset]);
            else
               printf ("                       %s\n", &bufp[offset]);

            offset += strlen(&bufp[offset]) + 1;
         }

         free (bufp);
      }
   }

   if (stat == RTERR_ENDOFBUF || stat == RTERR_ENDOFFILE) stat = 0;

   if (stat != 0) {
      printf ("dump failed; status = %d\n", stat);
   }

   return (stat);
}

static char* fmt_contents (FILE* fp, int len, int *count)
{
   int	segmentLen  = (NUM_SEGMENT_BYTES * 4) + 1;
   int	numSegments = ((len - 1))/NUM_SEGMENT_BYTES + 1;
   int	i, j, offset, num_bytes;
   char	*bufp, *hexp, *ascp, buf[3];
   OSOCTET b;

   *count = 0;

   if (!(bufp = (char *) malloc (numSegments * (segmentLen+1))))
      return (NULL);

   for (i = 0, offset = 0; i < numSegments; 
        i++, offset += (segmentLen+1), len -= NUM_SEGMENT_BYTES)
   {
      memset (&bufp[offset], ' ', segmentLen);
      bufp[offset+segmentLen] = '\0';

      hexp = &bufp[offset];
      ascp = &bufp[offset+(NUM_SEGMENT_BYTES*3+1)];

      num_bytes = MIN (len, NUM_SEGMENT_BYTES);

      for (j = 0; j < num_bytes; j++)
         if (fread (&b, 1, 1, fp) == 1) {
            sprintf (buf, "%02x", b);
            *hexp++ = buf[0];
            *hexp++ = buf[1];
            *hexp++ = ' ';
            *ascp++ = (b > 31 && b < 128) ? b : '.';
         }
         else break;
   }

   *count = i;

   return (bufp);
}

