/* Copyright (c) Oracle Corporation, 1996.  All Rights Reserved.    */

/*******************************************************************************
*
*   NAME
*
*       smpup130.xrl
*
*   DESCRIPTION
*
*       Upgrade script for console 
*
*   NOTES
*
*       Upgrades v1.2 repository to V1.3
*
*******************************************************************************/

/*******************************************************************************
**  Local variables
*******************************************************************************/

xp_str_t user, pwd, service, buf, path;

/*******************************************************************************
**  Convert Perf. Mgr blobs to 1.3 format
*******************************************************************************/

repos::user_info (user, pwd, service);

path = getpath ("ORACLE_HOME");

sprintf(buf,'%1BIN\VMMUPGR %2 %3 %4', path, user, pwd, service);

shell (buf);

repos::work_in_progress();

/*******************************************************************************
**  Update repository version
*******************************************************************************/

repos::update_version ("CONSOLE");
