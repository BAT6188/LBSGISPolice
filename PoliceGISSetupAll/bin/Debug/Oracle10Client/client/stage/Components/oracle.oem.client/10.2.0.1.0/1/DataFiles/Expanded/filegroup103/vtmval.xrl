/* Copyright (c) Oracle Corporation, 1998.  All Rights Reserved.    */

/*******************************************************************************
*
*   NAME
*
*       vtmval.xrl
*
*   DESCRIPTION
*
*       Validation script for Oracle Performance Manager.
*
*******************************************************************************/

/*******************************************************************************
**  First, check the common validation
*******************************************************************************/

drop_steps = 3;
create_steps = 3;

xp_int_t status = repos::common_validation("VTM");
if (status != VOBMGR_K_NOT_FOUND)
  return (status);
