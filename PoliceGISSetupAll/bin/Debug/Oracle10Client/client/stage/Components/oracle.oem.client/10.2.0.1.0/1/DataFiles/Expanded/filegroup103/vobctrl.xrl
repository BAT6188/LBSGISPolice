/* Copyright (c) Oracle Corporation, 1998, 2000.  All Rights Reserved.    */

/*******************************************************************************
*
*   NAME
*
*       vobctrl.xrl
*
*   DESCRIPTION
*
*       Repository manager control script for OEM.  This is the main driver
*       script for setting up auto-management of the repository.
*
*   NOTES
*
*******************************************************************************/

/*******************************************************************************
**  Setup
**
**      The variable 'current_version' must be changed when ever the
**      product version changes.
**
*******************************************************************************/

current_version = "2.2";

xp_str_t name;

migrate_steps = 24;

/*******************************************************************************
**  Repository management for Enterprise manager
*******************************************************************************/

name = "Enterprise Manager";

repos::product (name);

    repos::group (name, "ALL");
    repos::group (name, "CONSOLE");
    repos::group (name, "DIAG");
    repos::group (name, "OCM");

    /* upgrade the version from 1.5 to 1.6 */
    repos::version (name, "1.6");

    repos::xrl_script (name, VALIDATE, "smpval.xrl");

    repos::sql_script (name, CREATE,   "smpcre.xdl", SOURCE);
    repos::xrl_script (name, CREATE,   "smpup.xrl");

    repos::sql_script (name, DROP,     "smpdrp.xdl", SOURCE);
    repos::xrl_script (name, DROP,     "smpdrp.xrl");

    repos::xrl_script (name, MIGRATE,  "smpmival.xrl");
    /*dummy script for avoiding error*/
    repos::sql_script (name, MIGRATE,  "${vobmgr.home}migration\\v22\\vdvcreatedummy_emma.sql", TARGET);
    repos::xrl_script (name, MIGRATE,  "smpmiutl.xrl");
    repos::sql_script (name, MIGRATE,  "smpmivdn.sql", SOURCE);
    repos::xrl_script (name, MIGRATE,  "smpmivdn.xrl");
    repos::sql_script (name, MIGRATE,  "smpmivdv.sql", SOURCE);
    repos::xrl_script (name, MIGRATE,  "smpmivdv.xrl");
    repos::sql_script (name, MIGRATE,  "smpmivdj.sql", SOURCE);
    repos::xrl_script (name, MIGRATE,  "smpmivdj.xrl");
    repos::sql_script (name, MIGRATE,  "smpmivde.sql", SOURCE);
    repos::xrl_script (name, MIGRATE,  "smpmivde.xrl");
    repos::xrl_script (name, MIGRATE,  "smpmivar.xrl");
    repos::sql_script (name, MIGRATE,  "smpmidrp.sql", SOURCE);
    /*Actions to upgrade from 2.1.0 to 2.1.0.1-----*/
    repos::sql_script (name, MIGRATE,  "${vobmgr.home}migration\\v21\\vdgsecmigrate.sql", TARGET);
    /* ------Actions to upgrade from 2.1.0.1 to 2.2.0.0---------*/
    /*repos::sql_script (name, MIGRATE,  "${vobmgr.home}migration\\v22\\vbormigrate.sql", TARGET);*/
    repos::sql_script (name, MIGRATE,  "${vobmgr.home}migration\\v22\\vdvmigrate_emma.sql", TARGET);
    repos::sql_script (name, MIGRATE,  "${vobmgr.home}migration\\v22\\vdnmigrate_emma.sql", TARGET);
    /*repos::sql_script (name, MIGRATE,  "${vobmgr.home}migration\\v22\\vdmmigrate.sql", TARGET);
    repos::sql_script (name, MIGRATE,  "${vobmgr.home}migration\\v22\\vdemigrate.sql", TARGET);
    repos::sql_script (name, MIGRATE,  "${vobmgr.home}migration\\v22\\vdgmigrate.sql", TARGET);
    repos::sql_script (name, MIGRATE,  "${vobmgr.home}migration\\v22\\vtadbappmigrate.sql", TARGET);
    repos::sql_script (name, MIGRATE,  "${vobmgr.home}migration\\v22\\vxaappmigrate.sql", TARGET);
    repos::sql_script (name, MIGRATE,  "${vobmgr.home}migration\\v22\\vdimigrate.sql", TARGET);*/
    repos::xrl_script (name, MIGRATE,  "SMPExeJavaScripts.xrl");
   
    repos::sql_script (name, UPGRADE,  "smpup120.sql", "1.1",   "1.2");
    repos::xrl_script (name, UPGRADE,  "smpup.xrl",    "1.1",   "1.2");
    repos::xrl_script (name, UPGRADE,  "smpup.xrl",    "1.2",   "1.2.1");
    repos::xrl_script (name, UPGRADE,  "smpup.xrl",    "1.2.1", "1.2.2");
    repos::sql_script (name, UPGRADE,  "smpup130.sql", "1.2.2", "1.3");
    repos::xrl_script (name, UPGRADE,  "smpup130.xrl", "1.2.2", "1.3");
    repos::xrl_script (name, UPGRADE,  "smpup140.xrl", "1.3",   "1.4");
    repos::sql_script (name, UPGRADE,  "smpup150.sql", "1.4",   "1.5");
    repos::xrl_script (name, UPGRADE,  "smpup.xrl",    "1.4",   "1.5");
    repos::xrl_script (name, UPGRADE,  "smpup160.xrl", "1.5",   "1.6");
    repos::xrl_script (name, UPGRADE,  "smpup.xrl",    "1.5",   "1.6");


/*******************************************************************************
**  Repository management for Oracle Expert for Oracle
*******************************************************************************/

name = "Oracle Expert";

repos::product (name);

    repos::version (name, "2.2");
    
    repos::group(name, "ALL");
    repos::group(name, "TUNE");
    repos::group(name, "TUNEONLY");

    repos::xrl_script (name, VALIDATE, "vdkval.xrl");

    repos::xrl_script (name, VALIDATE, "vmqval.xrl");

    repos::sql_script (name, CREATE,   "${vobmgr.home}migration\\v21\\vdkcr210.sql", TARGET);
    repos::sql_script (name, CREATE,   "${vobmgr.home}migration\\v21\\vdkidx210.sql", TARGET);
    repos::xrl_script (name, CREATE,   "vdkup.xrl");

    repos::sql_script (name, CREATE,   "vmqcre.sql", TARGET);
    repos::xrl_script (name, CREATE,   "vmqup.xrl");

    repos::xrl_script (name, DROP,     "vdkdr204.xrl");
    repos::xrl_script (name, DROP,     "vdkdr210.xrl");
    repos::xrl_script (name, DROP,     "vdkdr.xrl");

    repos::sql_script (name, DROP,     "vmqdrp.sql", TARGET);
    repos::xrl_script (name, DROP,     "vmqdr.xrl");

    repos::xrl_script (name, MIGRATE,  "xpmival.xrl");
    repos::xrl_script (name, MIGRATE,  "xpomigr.xrl");

    repos::xrl_script (name, MIGRATE,  "vmqmigr.xrl");

    repos::xrl_script (name, UPGRADE,  "xpoup130.xrl",      "1.2", "1.3");
    repos::xrl_script (name, UPGRADE,  "xpoup140.xrl",      "1.3", "1.4");
    repos::xrl_script (name, UPGRADE,  "xpoup150.xrl",      "1.4", "1.5");
    repos::xrl_script (name, UPGRADE,  "xpoup160.xrl",      "1.5", "1.6");
    repos::sql_script (name, UPGRADE,  "xpoup200.sql",      "1.6", "2.0");
    repos::xrl_script (name, UPGRADE,  "xpoup200.xrl",      "1.6", "2.0");
    repos::xrl_script (name, UPGRADE,  "xpdridx.xrl",       "1.6", "2.0");
    repos::sql_script (name, UPGRADE,  "xpidx.sql",         "1.6", "2.0");
    repos::xrl_script (name, UPGRADE,  "xpoup.xrl",         "1.6", "2.0");
    repos::xrl_script (name, UPGRADE,  "vdkdr210.xrl",      "2.0", "2.1");
    repos::sql_script (name, UPGRADE,  "${vobmgr.home}migration\\v21\\vdkcr210.sql",      "2.0", "2.1");
    repos::sql_script (name, UPGRADE,  "${vobmgr.home}migration\\v21\\vdkup210.sql",      "2.0", "2.1");
    repos::sql_script (name, UPGRADE,  "${vobmgr.home}migration\\v21\\vdkidx210.sql",     "2.0", "2.1");
    repos::xrl_script (name, UPGRADE,  "vdkup210.xrl",      "2.0", "2.1");
    repos::xrl_script (name, UPGRADE,  "vdkup.xrl",         "2.0", "2.1");

    repos::xrl_script (name, UPGRADE,  "vmqup160.xrl",      "1.5.5", "1.6");
    repos::xrl_script (name, UPGRADE,  "vmqup.xrl",         "1.5.5", "1.6");
    repos::sql_script (name, UPGRADE,  "${vobmgr.home}migration\\v22\\vmqup220.sql",      "2.1",   "2.2");

/*******************************************************************************
**  Repository management for Oracle Trace
*******************************************************************************/

name = "Oracle Trace";

repos::product (name);

    repos::group(name, "ALL");
    repos::group(name, "DIAG");
    repos::group(name, "DIAGONLY");
    repos::version (name, "1.6.5");


    repos::xrl_script (name, VALIDATE, "epcval.xrl");

    repos::sql_script (name, CREATE,   "epccre.sql", TARGET);
    repos::xrl_script (name, CREATE,   "epcup.xrl");

    repos::sql_script (name, DROP,     "epcdrp.sql", TARGET);
    repos::xrl_script (name, DROP,     "epcdrp.xrl");

    repos::xrl_script (name, MIGRATE,  "epcmival.xrl");
    repos::xrl_script (name, MIGRATE,  "epcmigr.xrl");

    repos::sql_script (name, UPGRADE,  "epcup130.sql", "1.2", "1.3");
    repos::xrl_script (name, UPGRADE,  "epcup.xrl",    "1.2", "1.3");
    repos::sql_script (name, UPGRADE,  "epcup140.sql", "1.3", "1.4");
    repos::xrl_script (name, UPGRADE,  "epcup.xrl",    "1.3", "1.4");
    repos::sql_script (name, UPGRADE,  "epcup150.sql", "1.4", "1.5");
    repos::xrl_script (name, UPGRADE,  "epcup.xrl",    "1.4", "1.5");
    repos::sql_script (name, UPGRADE,  "epcup155.sql", "1.5", "1.5.5");
    repos::xrl_script (name, UPGRADE,  "epcup.xrl",    "1.5", "1.5.5");
    repos::xrl_script (name, UPGRADE,  "epcup160.xrl", "1.5.5", "1.6");
    repos::xrl_script (name, UPGRADE,  "epcup.xrl",    "1.5.5", "1.6");
    repos::sql_script (name, UPGRADE,  "epcup165.sql", "1.6", "1.6.5");
    repos::xrl_script (name, UPGRADE,  "epcup.xrl",    "1.6", "1.6.5");


/*******************************************************************************
**  Repository management for Oracle Trace formatted databases
*******************************************************************************/

name = "Trace Formatted DB";

repos::product (name);

    repos::group (name, "ALL");
    repos::group (name, "EPCFMT");

    /* Leave version at 1.4 since there are no changes for 1.5 */
    repos::version (name, "1.4");

    repos::xrl_script (name, VALIDATE, "epcfmtv.xrl");

    repos::sql_script (name, CREATE,   "epcfmtc.sql", TARGET);
    repos::xrl_script (name, CREATE,   "epcfmtup.xrl");

    repos::xrl_script (name, DROP,     "epcfmtd.xrl");

    repos::sql_script (name, UPGRADE,  "epcf140.sql",  "1.2", "1.4");
    repos::sql_script (name, UPGRADE,  "epcfmtc.sql",  "1.2", "1.4");
    repos::xrl_script (name, UPGRADE,  "epcf140.xrl",  "1.2", "1.4");
    repos::xrl_script (name, UPGRADE,  "epcfmtdo.xrl",  "1.2", "1.4");
    repos::xrl_script (name, UPGRADE,  "epcfmtup.xrl", "1.2", "1.4");


/*******************************************************************************
**  Repository management for Oracle SQL Analyze (moved to Oracle Expert region)
*******************************************************************************

name = "SQL Analyze";

repos::product (name);

    repos::version (name, "2.0");

    repos::group(name, "ALL");
    repos::group(name, "TUNE");
    repos::group(name, "TUNEONLY");

    repos::xrl_script (name, VALIDATE, "vmqval.xrl");

    repos::sql_script (name, CREATE,   "vmqcreat.sql", TARGET);
    repos::xrl_script (name, CREATE,   "vmqup.xrl");

    repos::sql_script (name, DROP,     "vmqdrop.sql", TARGET);
    repos::xrl_script (name, DROP,     "vmqdr.xrl");

    repos::xrl_script (name, MIGRATE,  "vmqmival.xrl");
    repos::xrl_script (name, MIGRATE,  "vmqmigr.xrl");

    repos::xrl_script (name, UPGRADE,  "vmqup160.xrl", "1.5.5", "1.6");
    repos::xrl_script (name, UPGRADE,  "vmqup.xrl",    "1.5.5", "1.6");

*******************************************************************************
**  Repository management for Oracle OCM
*******************************************************************************/

name = "Change Manager";

repos::product (name);

    repos::group(name, "ALL");
    repos::group(name, "OCM");
    repos::group(name, "OCMONLY");
    repos::version (name, "1.6");
    
    repos::xrl_script (name, VALIDATE, "ocmval.xrl");

    repos::sql_script (name, CREATE,   "ocm21cre.sql", TARGET);
    repos::xrl_script (name, CREATE,   "ocmup.xrl");

    repos::sql_script (name, DROP,     "ocm21drp.sql", TARGET);
    repos::xrl_script (name, DROP,     "ocmdrp.xrl");

    repos::xrl_script (name, MIGRATE,  "ocmmival.xrl");
    repos::sql_script (name, MIGRATE,  "ocm21drp.sql", TARGET);
    repos::sql_script (name, MIGRATE,  "ocmcre.sql", TARGET);
    repos::xrl_script (name, MIGRATE,  "ocmmigr.xrl");
    repos::sql_script (name, MIGRATE,  "${vobmgr.home}migration\\v21\\ocm21rn_migrate.sql", TARGET);
    repos::sql_script (name, MIGRATE,  "${vobmgr.home}migration\\v21\\ocm21cre_migrate.sql", TARGET);
    repos::sql_script (name, MIGRATE,  "${vobmgr.home}migration\\v21\\ocm21pop1_migrate.sql", TARGET);
    repos::xrl_script (name, MIGRATE,  "${vobmgr.home}migration\\v21\\OCMLongsMigrate.xrl");
    repos::sql_script (name, MIGRATE,  "${vobmgr.home}migration\\v21\\ocm21pop3_migrate.sql", TARGET);
    repos::sql_script (name, MIGRATE,  "${vobmgr.home}migration\\v21\\ocm21dr_migrate.sql", TARGET);
 
    repos::sql_script (name, UPGRADE,  "ocmdrp.sql",  "0.1.0", "1.5.5");
    repos::sql_script (name, UPGRADE,  "ocmcre.sql",  "0.1.0", "1.5.5");
    repos::xrl_script (name, UPGRADE,  "ocmup160.xrl","1.5.5", "1.6");
    repos::xrl_script (name, UPGRADE,  "ocmup.xrl",   "1.5.5", "1.6");

/*******************************************************************************
**  Repository management for Oracle Performance Manager V2
*******************************************************************************/

name = "Performance Manager";

repos::product (name);

    repos::group(name, "ALL");
    repos::group(name, "DIAG");
    repos::group(name, "DIAGONLY");

    repos::xrl_script (name, VALIDATE, "vtmval.xrl");

    repos::sql_script (name, CREATE,   "vtmcre.sql", TARGET);
    repos::xrl_script (name, CREATE,   "vtmup.xrl");

    repos::sql_script (name, DROP,     "vtmdrp.sql", TARGET);
    repos::xrl_script (name, DROP,     "vtmdrp.xrl");

    repos::xrl_script (name, MIGRATE,  "vtmmival.xrl");
    repos::xrl_script (name, MIGRATE,  "vtmmigr.xrl");

    repos::sql_script (name, UPGRADE,  "vtmup160.sql", "1.5.5", "1.6");
    repos::xrl_script (name, UPGRADE,  "vtmup.xrl", "1.5.5", "1.6");
    repos::xrl_script (name, UPGRADE,  "vtmup.xrl", "1.6", "2.1");

/*******************************************************************************
**  Repository management for Oracle Tablespace Manager
*******************************************************************************/

name = "Tablespace Manager";

repos::product (name);

    repos::xrl_script (name, VALIDATE, "vmtval.xrl");

    repos::sql_script (name, CREATE,   "vmtcre.sql", TARGET);
    repos::xrl_script (name, CREATE,   "vmtup.xrl");

    repos::sql_script (name, DROP,     "vmtdrp.sql", TARGET);
    repos::xrl_script (name, DROP,     "vmtdrp.xrl");

    repos::xrl_script (name, MIGRATE,  "vmtmival.xrl");
    repos::xrl_script (name, MIGRATE,  "vmtmigr.xrl");
