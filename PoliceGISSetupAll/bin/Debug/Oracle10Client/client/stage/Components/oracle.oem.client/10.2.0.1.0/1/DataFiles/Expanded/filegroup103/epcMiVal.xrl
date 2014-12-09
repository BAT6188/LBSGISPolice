/* Copyright (c) Oracle Corporation, 1998, 2000.  All Rights Reserved.    */

/*******************************************************************************
*
*   NAME
*
*       epcmival.xrl
*
*   DESCRIPTION
*
*       Validation script for the target repository of an Oracle Trace
*       migration.  
*
*   NOTES
*
*******************************************************************************/

xp_int_t status = target::common_validation("EPC");
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

target::update_version("EPC");

return (VOBMGR_K_OK);
