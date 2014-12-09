/* Copyright (c) Oracle Corporation, 1998.  All Rights Reserved.    */

/*******************************************************************************
*
*   NAME
*
*       xpmival.xrl
*
*   DESCRIPTION
*
*       Validation script for the target repository of an Oracle Expert 
*       migration.  
*
*   NOTES
*
*******************************************************************************/

xp_int_t status = target::common_validation("EXPERT");
if (status != VOBMGR_K_OK)
  {
    if (status == VOBMGR_K_NOT_FOUND)
      {
        return (VOBMGR_K_TARGET_CREATE);
      }
  }

/*******************************************************************************
**  We must be ok - update the version in smp_vds_repos_version.
*******************************************************************************/

target::update_version("EXPERT");

return (VOBMGR_K_OK);
