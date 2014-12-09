/* Copyright (c) Oracle Corporation, 1994, 1986.  All Rights Reserved.    */

/*******************************************************************************
*
*   NAME
*
*      xpomiwrk.xrl
*
*   DESCRIPTION
*
*      Migrate Expert worker script
*
*   NOTES
*
*******************************************************************************/

xp_str_t src_conn, trg_conn, em_user;

if (gl_parameters[3] == "")
  printf("Error: Missing arguments");

set output gl_parameters[3]

add_package ("xpcrto_tune_session", "xpcrto");
enable_package("*");

src_conn = gl_parameters[0];
trg_conn = gl_parameters[1];
em_user = gl_parameters[2];

Tune::Migrate (src_conn, trg_conn, em_user);
