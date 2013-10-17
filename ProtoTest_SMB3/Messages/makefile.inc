!INCLUDE $(INETROOT)\build\makefile.inc

NEW_RESOURCES_DIRS=$(NEW_RESOURCES_DIRS:  = )
NEW_RESOURCES_DIRS=$(NEW_RESOURCES_DIRS) fakedir
TMP_RESOURCES_RULES=$(O)\resx_rule.inc

insert_rules: 
!if "$(BUILD_PASS)"=="PASS0"
	echo Build_Status Inserting rules to make resources
!if "$(NEW_RESOURCES_NAMESPACE)"!=""
	type <<$(TMP_RESOURCES_RULES)
{$(NEW_RESOURCES_DIRS: =\}.resx{$(O^)\}.resources:
    $(RESGEN_CMD^) $(USER_RESGEN_FLAGS^) $< $(@D^)\$(NEW_RESOURCES_NAMESPACE^).$(@F^) > $(O^)\resgen.log 
{)\}.resx{$$(O)\}.resources:
    @rem fake rule
<<KEEP
!else
	type <<$(TMP_RESOURCES_RULES)
{$(NEW_RESOURCES_DIRS: =\}.resx{$(O^)\}.resources:
    $(RESGEN_CMD^) $(USER_RESGEN_FLAGS^) $< $@ > $(O^)\resgen.log 
{)\}.resx{$$(O)\}.resources:
    @rem fake rule 
<<KEEP
!endif
!endif

!if EXIST($(TMP_RESOURCES_RULES))
!include $(TMP_RESOURCES_RULES)
!endif

