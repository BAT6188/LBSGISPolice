/* Copyright (c) Oracle Corporation, 1996.  All Rights Reserved.    */

/*******************************************************************************
*
*   NAME
*
*       xpoval.xrl
*
*   DESCRIPTION
*
*       Validation script for Oracle SQL Expert
*
*******************************************************************************/

/*******************************************************************************
**  First, check the common validation
*******************************************************************************/

drop_steps = 12;
create_steps = 12;
upgrade_steps = 14;

xp_int_t status = repos::common_validation("SQL Analyze");
if (status != VOBMGR_K_NOT_FOUND)
  return (status);
