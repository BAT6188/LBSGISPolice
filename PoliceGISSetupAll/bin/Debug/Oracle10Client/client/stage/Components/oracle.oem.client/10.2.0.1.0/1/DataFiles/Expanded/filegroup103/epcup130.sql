rem  NAME
rem	epcupg.sql
rem
rem	Copyright (c) 1996, 2000 by Oracle Corporation
rem
rem  FUNCTION
rem	Upgrade script for Oracle Trace
rem
rem  NOTES
rem
rem	This upgrade is used for either 1.1 or 1.2 databases
rem 

rem	drop the old tables
@otrccatd.sql

rem	create the new tables
@epccre13.sql


