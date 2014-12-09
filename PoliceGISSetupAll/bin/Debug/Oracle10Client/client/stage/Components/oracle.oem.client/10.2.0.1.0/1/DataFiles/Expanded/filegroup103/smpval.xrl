/* Copyright (c) Oracle Corporation, 1996.  All Rights Reserved.    */

/*******************************************************************************
*
*   NAME
*
*       smpval.xrl
*
*   DESCRIPTION
*
*       Validation script for Enterprise Manager
*
*******************************************************************************/

/*******************************************************************************
**  Just call the common validation routine
*******************************************************************************/

upgrade_steps = 21;

xp_int_t status = repos::common_validation2 ("CONSOLE", "/com/oracle/Sysman/EM/EMSystem");
if (status != VOBMGR_K_NOT_FOUND)
  return (status);

/*******************************************************************************
**  Look for 1.1 repository
*******************************************************************************/

if (repos::table_exists ("smp_blob"))
  {
    repository_version = "1.1";

    return (VOBMGR_K_UPGRADE);
  }

/*******************************************************************************
**  Default to not found
*******************************************************************************/

return (VOBMGR_K_NOT_FOUND);
