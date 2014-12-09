/* Copyright (c) Oracle Corporation, 1998.  All Rights Reserved.    */

/*******************************************************************************
*
*   NAME
*
*       smpup160.xrl
*
*   DESCRIPTION
*
*       Upgrade script for console 
*
*   NOTES
*
*       Upgrades v1.5 repository to V1.6
*
*   MODIFIED
*	$Log: /OEM_1.6.0/ADMIN/SMPUP160.XRL $
******************************************************************/

/*****************************************************************
*  1.5 event system repository into a 1.6
******************************************************************/

repos::execute("alter table EVT_PROFILE" 
        " modify (PROFILE_NAME	        VARCHAR2(80),"
                "PROFILE_DESCRIPTION	VARCHAR2(2000) )");

repos::work_in_progress();

