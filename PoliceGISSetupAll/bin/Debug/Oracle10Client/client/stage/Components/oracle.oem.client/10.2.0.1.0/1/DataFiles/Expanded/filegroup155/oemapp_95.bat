@echo off
REM #
REM # Copyright (c) 1998, 1999, 2000, 2001, 2002 Oracle Corporation.  All rights reserved.
REM #
REM # PRODUCT
REM #	Oracle Enterprise Manager, Version 9.2.0.0.0 Production
REM #
REM # FILENAME
REM #	oemapp.bat
REM #
REM # DESCRIPTION
REM #	This script is used to launch Applications of Oracle
REM #	Enterprise Manager, Version 9
REM #
REM #

if "%1%" == "recursive" goto startScript

if Windows_NT == %OS% goto :ntSetup

command.com /p /e:8196 /c %ORACLE_HOME%\bin\oemapp recursive %1 %2 %3 %4 %5 %6 %7 %8 %9
goto :end

:ntSetup
setlocal

:startScript

IF "%2%" == "trace" GOTO trace
IF "%2%" == "TRACE" GOTO trace
IF "%2%" == "debug" GOTO trace
IF "%2%" == "DEBUG" GOTO trace
GOTO start2 

:trace
SHIFT 
SET ORACLE_OEM_CLIENTTRACE=TRUE 

:start2
REM Make sure that our JRE is used for this invocation.
if Windows_NT == %OS% SET PATH=%JRE_LOCATION%;%ORACLE_HOME%\bin;%PATH%
if not Windows_NT == %OS% SET PATH="%JRE_LOCATION%;%ORACLE_HOME%\bin;%PATH%"

rem although not used directly in this script, this variable is required by
rem launchem, which checks for this environment variable.
SET OHOME=%ORACLE_HOME%

SET CLASSROOT=classes
SET JLIBROOT=jlib
SET OEMCLASSES=%JLIBROOT%\oembase-%OEM_VER%.jar;%JLIBROOT%\oemtools-%OEM_VER%.jar
SET NETJLIBROOT=network\jlib
SET BALICLASSES=%JLIBROOT%\swingall-1_1_1.jar;%JLIBROOT%\ewtcompat-3_3_15.jar;%JLIBROOT%\ewt3.jar;%JLIBROOT%\ewt3-nls.jar;%JLIBROOT%\share.jar;%HELP_FILES%
SET AURORACLASSES=jlib\aurora_client.jar
SET HELPCLASSES=%JLIBROOT%\help4.jar;%JLIBROOT%\help4-nls.jar;%JLIBROOT%\oracle_ice5.jar
SET DBUICLASSES=%JLIBROOT%\dbui2.jar;%JLIBROOT%\dbui2-nls.jar
SET KODIAKCLASSES=%JLIBROOT%\kodiak.jar
SET JDBCCLASSES=jdbc\lib\ojdbc14.jar;jdbc\lib\nls_charset12.zip;jlib\orai18n.jar
SET NETCHARTSCLASS=sysman\jlib\netchart360.jar
SET ORBCLASSES=lib/vbjorb.jar;lib/vbjapp.jar
SET SECURITYCLASSES=%JLIBROOT%\javax-ssl-1_1.jar;%JLIBROOT%\jssl-1_1.jar
SET OSDNETCLASSES=%JLIBROOT%\netcfg.jar;%JLIBROOT%\o3logon.jar;%JLIBROOT%\verifier14.jar
SET OSDPKICLASSES=%JLIBROOT%\ewm-1_1.jar;%JLIBROOT%\ojpse_2_1_5.jar
SET NETMGRCLASSES=%NETJLIBROOT%/netmgrm.jar;%NETJLIBROOT%/netmgr.jar;network/tools
SET XMLCLASSES=lib\xmlparserv2.jar
SET OLAPCLASSES=%JLIBROOT%/cvd.zip;%JLIBROOT%/jndi.jar;%JLIBROOT%/jewt4.jar;%JLIBROOT%/jewt4-nls.jar
SET MISCCLASSES=assistants/jlib/assistantsCommon.jar;%JLIBROOT%/jle2.jar;%JLIBROOT%/jle2-nls.jar;%JLIBROOT%/ldap.jar;%JLIBROOT%/ldapjclnt10.jar
SET QSMACLASSES=rdbms\jlib\qsma.jar

SET CLASSPATHADD=%CLASSROOT%;%OEMCLASSES%;%ORBCLASSES%;%BALICLASSES%;%HELPCLASSES%;%DBUICLASSES%;%JDBCCLASSES%;%KODIAKCLASSES%;%NETCHARTSCLASS%;%SECURITYCLASSES%;%OSDNETCLASSES%;%OSDPKICLASSES%;%NETMGRCLASSES%;%XMLCLASSES%;%AURORACLASSES%;%OLAPCLASSES%;%MISCCLASSES%;%QSMACLASSES%

REM hook for classpath additions
if not "%ORACLE_OEM_CLASSPATH%" == "" SET CLASSPATHADD=%ORACLE_OEM_CLASSPATH%;%CLASSPATHADD%


REM # Oracle change manager requires the following for ocmtclsh.exe
set OCM_LIBRARY=%ORACLE_HOME%/network/agent/tcl
set GBP=
if not exist %CLASSROOT%\oracle\sysman\vtt\vttz\VttzCmDragDropObject.class set GBP=true


if "%ORACLE_OEM_JAVAMX%" == "" set ORACLE_OEM_JAVAMX=-mx128m
REM if "%ORACLE_OEM_JAVAMS%" == "" set ORACLE_OEM_JAVAMS=-ms16m
SET JRE=java %ORACLE_OEM_JAVAMX%
SET NT_START=START
if "%ORACLE_OEM_CLIENTTRACE%x" == "x" goto setup_cp
SET JRE=java %ORACLE_OEM_JAVAMX% %TRACE%
SET NT_START=

:setup_cp
SET CLASSPATH_QUAL=cp

if "%ORACLE_OEM_JAVARUNTIME%x" == "x" goto jreSetup
SET JRE=%ORACLE_OEM_JAVARUNTIME%\bin\java %ORACLE_OEM_JAVAMX% %TRACE%
SET CLASSPATH_QUAL=classpath
SET CLASSPATHADD=%CLASSPATHADD%;%ORACLE_OEM_JAVARUNTIME%\lib\classes.zip

:jreSetup
SET ME=oemapp
SET USAGE="Usage: %ME% <application_name>"

if Windows_NT == %OS% goto ntCheckAppName
if not  "%2%" == "" goto start95
@echo %USAGE%
goto end

:start95
%ORACLE_DRIVE%
cd %ORACLE_HOME%
set JREOPTIONS_STRING= -Djdbc.backward_compatible_to_816
bin\launchem.exe %2 %3 %4 %5 %6 %7 %8 %9
rem use this line to start em instead of bin\launchem above to get trace output
rem %NT_START% %JRE% -Dsun.hava2d.noddraw=true -DORACLE_HOME=%ORACLE_HOME% -DTRACING.ENABLED=TRUE -DTRACING.LEVEL=2 %GBP_PROP% -DORBdisableLocator=true -Djdbc.backward_compatible_to_816=true %JREOPTIONS_STRING% -%CLASSPATH_QUAL% "%CLASSPATHADD%" oracle.sysman.vtx.vtxOemApp.OemApp %2 %3 %4 %5 %6 %7 %8 %9
cd bin
goto end

:ntCheckAppName
if not  "%1%" == "" goto startNT
@echo %USAGE%
goto end

:startNT
cd /d %ORACLE_HOME%
set GBP_PROP=
if "%GBP%" == "true" set GBP_PROP=-DGENERAL_BUS_PACK=""
%NT_START% %JRE% -Dsun.hava2d.noddraw=true -DORACLE_HOME=%ORACLE_HOME% %GBP_PROP% -DORBdisableLocator=true -Djdbc.backward_compatible_to_816=true %JREOPTIONS_STRING% -%CLASSPATH_QUAL% "%CLASSPATHADD%" oracle.sysman.vtx.vtxOemApp.OemApp %1 %2 %3 %4 %5 %6 %7 %8 %9
endLocal
:end

