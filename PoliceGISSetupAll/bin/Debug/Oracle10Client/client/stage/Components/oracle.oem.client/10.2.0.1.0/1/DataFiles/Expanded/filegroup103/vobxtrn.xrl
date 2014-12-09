/* Copyright (c) Oracle Corporation, 1996.  All Rights Reserved.    */

/*******************************************************************************
*
*   NAME
*
*       vobxtrn.xrl
*
*   DESCRIPTION
*
*       Repository manager control script for OEM 3rd-party integration.  
*       This is the main driver script for setting up auto-management of 
*       3rd party repository components.
*
*   NOTES
*
*******************************************************************************/

/*******************************************************************************
**  Setup
**
*******************************************************************************/

xp_str_t name;

/*******************************************************************************
**  EXAMPLE - Repository management for MyProduct 
**
**  NOTE - the entire example is hidden within a comment block!
*******************************************************************************/

/*******************************************************************************
**  
**  name = "Oracle Expert";
**  
**  repos::product (name);
**  
**      repos::group(name, "ALL");
**      repos::group(name, "PERFORMANCE");
**  
**      repos::version(name, "1.4");
**  
**      repos::xrl_script (name, VALIDATE, "xpoval.xrl");
**  
**      repos::sql_script (name, CREATE,   "xpocr.sql");
**      repos::xrl_script (name, CREATE,   "xpoup.xrl");
**  
**      repos::sql_script (name, DROP,     "xpodr.sql");
**      repos::xrl_script (name, DROP,     "xpodr.xrl");
**  
**      repos::xrl_script (name, UPGRADE,  "xpoup130.xrl", "1.2", "1.3");
**      repos::xrl_script (name, UPGRADE,  "xpoup.xrl",    "1.2", "1.3");
**      repos::xrl_script (name, UPGRADE,  "xpoup140.xrl", "1.3", "1.4");
**      repos::xrl_script (name, UPGRADE,  "xpoup.xrl",    "1.3", "1.4");
**  
*******************************************************************************/


/*******************************************************************************
**  Repository management for Oracle Security Server
*******************************************************************************/

name = "Oracle Security Server";

repos::product (name);

    repos::version (name, "2.0.3");

    repos::xrl_script (name, VALIDATE, "${vobroot}OSS/ossval.xrl");
    repos::sql_script (name, CREATE,   "${vobroot}OSS/nzdoadm.sql", TARGET);
    repos::sql_script (name, DROP,     "${vobroot}OSS/nzdodrop.sql", TARGET);


name = "OSS-SYSTEM";
 
repos::product (name);
 
    repos::version (name, "2.0.3");
 
    repos::xrl_script (name, VALIDATE, "${vobroot}OSS/osssval.xrl");
    repos::sql_script (name, CREATE,   "${vobroot}OSS/nzdocrt.sql", TARGET);
    repos::sql_script (name, DROP,     "${vobroot}OSS/nzdodrop.sql", TARGET);

name = "Oracle Biometrics Manager";
 
repos::product (name);
 
    repos::version (name, "8.0.3");
 
    repos::xrl_script (name, VALIDATE, "${vobroot}IDENTIX/nauival.xrl");
    repos::sql_script (name, CREATE,   "${vobroot}IDENTIX/nauiadm.sql", TARGET);
    repos::sql_script (name, DROP,     "${vobroot}IDENTIX/nauidrop.sql", TARGET);
 
name = "naui-system";
 
repos::product (name);
 
    repos::version (name, "8.0.3");
 
    repos::xrl_script (name, VALIDATE, "${vobroot}IDENTIX/nauisval.xrl");
    repos::sql_script (name, CREATE,   "${vobroot}IDENTIX/nauicrt.sql", TARGET);
    repos::sql_script (name, DROP,     "${vobroot}IDENTIX/nauidrp.sql", TARGET);

