/* Copyright (c) Oracle Corporation, 1994, 1986.  All Rights Reserved.    */

/*******************************************************************************
*
*   NAME
*
*      xpomigr.xrl
*
*   DESCRIPTION
*
*      Migrate Expert tuning sessions from V1.n to V2.0
*
*   NOTES
*
*******************************************************************************/

xp_str_t command, src_conn, targ_conn, em_user;

xp_str_t script_path, log_path;

script_path = getpath("home");
strcat(script_path, "xpomiwrk.xrl");

log_path = getpath("scratch_path");
strcat(log_path, "xpomigr.log");

repos::repos_conn_str (src_conn);
repos::targ_conn_str (targ_conn);

repos::exec_migr_script (script_path, log_path, v2user, src_conn, targ_conn);