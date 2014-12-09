/* Copyright (c) Oracle Corporation, 1998.  All Rights Reserved.    */

/*******************************************************************************
*
*   NAME
*
*       vtmmival.xrl
*
*   DESCRIPTION
*
*       Validation script for the target repository of an Performance Manager
*       migration.  
*
*   NOTES
*
*******************************************************************************/

xp_int_t status = target::common_validation("VTM");
if (status != VOBMGR_K_OK)
  {
    if (status == VOBMGR_K_NOT_FOUND)
      {
        return (VOBMGR_K_TARGET_CREATE);
      }
  }

