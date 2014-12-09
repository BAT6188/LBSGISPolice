@echo off
REM #
REM # Copyright (c) 2002 Oracle Corporation.  All rights reserved.
REM #
REM # PRODUCT
REM #   Oracle Root Certificate Assistant, Version 10.1.0.0.0 Production
REM #
REM # FILENAME
REM #   mkRootCert.bat
REM #
REM # DESCRIPTION
REM #   This script is used to create a Root Cert for use in wallet creation.
REM #
REM # NOTE:
REM #   This script is typically invoked as follows:
REM #
REM #       mkRootCert
REM #

if "%1%" == "recursive" goto startScript

if Windows_NT == %OS% goto :ntSetup
command.com /p /e:4096 /c mkRootCert recursive %1
goto :end

:ntSetup
setlocal

:startScript

REM  Make sure that our JRE is used for this invocation.
if Windows_NT == %OS% SET PATH=%JRE_LOCATION%;%ORACLE_HOME%\bin;%PATH%
if not Windows_NT == %OS% SET PATH="%JRE_LOCATION%;%ORACLE_HOME%\bin;%PATH%"

SET WRL=%ORACLE_HOME%\sysman\admin
SET CLASSROOT=%ORACLE_HOME%\classes
SET JLIBROOT=%ORACLE_HOME%\jlib
SET OEMCLASSES=%JLIBROOT%/oemlt-%OEM_VER%.jar
SET SECURITYCLASSES=%JLIBROOT%/javax-ssl-1_1.jar;%JLIBROOT%/jssl-1_1.jar;%JLIBROOT%/ojpse_2_1_5.jar
SET OWMCLASSES=%ORACLE_HOME%/owm/jlib/owm-3_0.jar

SET CLASSPATHADD=%CLASSROOT%;%OEMCLASSES%;%SECURITYCLASSES%;%OWMCLASSES%

SET JRE=java
SET CLASSPATH_QUAL=cp

if "%ORACLE_OEM_JAVARUNTIME%x" == "x" goto jreSetup
SET JRE=%ORACLE_OEM_JAVARUNTIME%\bin\java
SET CLASSPATH_QUAL=classpath
SET CLASSPATHADD=%CLASSPATHADD%;%ORACLE_OEM_JAVARUNTIME%\lib\classes.zip

REM hook for classpath additions
if not "%ORACLE_OEM_CLASSPATH%" == "" SET CLASSPATHADD=%ORACLE_OEM_CLASSPATH%;%CLASSPATHADD%

:jreSetup

SET ME=mkRootCert
SET USAGE="Usage: %ME%"

REM Set the CA Identity.
CALL %ORACLE_HOME%\sysman\admin\esmca.properties.bat

REM allow the old key material to be rewritten
ATTRIB -R %WRL%\*.der

if Windows_NT == %OS% SET HOSTNAME=%COMPUTERNAME%
if not Windows_NT == %OS% SET HOSTNAME=ESM

SET JVMPROPS=-Desm.HOSTNAME=%HOSTNAME% -Desm.DC=%DOMAIN_COMPONENT% -Desm.COUNTRY=%COUNTRY% -Desm.STATE=%STATE% -Desm.LOC=%LOCALITY% -Desm.ORG=%ORGANIZATION% -Desm.ORGUNIT=%ORGANIZATION_UNIT% -Desm.EMAIL=%EMAIL_ADDR%

if Windows_NT == %OS% goto :ntStart
%JRE% %JVMPROPS% -DORACLE_HOME=%ORACLE_HOME% -%CLASSPATH_QUAL% "%CLASSPATHADD%" oracle.sysman.vd.euser.service.GenRootCert %WRL%
goto :end

:ntStart
%JRE% %JVMPROPS% -DORACLE_HOME=%ORACLE_HOME% -%CLASSPATH_QUAL% "%CLASSPATHADD%" oracle.sysman.vd.euser.service.GenRootCert %WRL%

:end
REM ensure that the key material is read only by the oracle user
ATTRIB +R %WRL%\*.der
endLocal
