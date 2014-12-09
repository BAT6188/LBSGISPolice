/* Copyright (c) Oracle Corporation, 1996.  All Rights Reserved.    */

/*******************************************************************************
*
*   NAME
*
*       vobval.xrl
*
*   DESCRIPTION
*
*       Validation script for repository control
*
*******************************************************************************/

/*******************************************************************************
**  Just call the common validation routine
*******************************************************************************/

drop_steps = 1;
create_steps = 1;
upgrade_steps = 1;

xp_int_t status = repos::common_validation ("CONTROL");
if (status != VOBMGR_K_NOT_FOUND)
  return (status);

/*******************************************************************************
**  Check for missing table
*******************************************************************************/

if (!repos::table_exists ("smp_rep_version"))
  {
    return (VOBMGR_K_NOT_FOUND);
  }

/*******************************************************************************
**  Existing repository with no entry for the control
*******************************************************************************/

if (!repos::integer("select count(*) from smp_rep_version"))
  {
    return (VOBMGR_K_NOT_FOUND);
  }

repos::select ("select c_current_version from smp_rep_version "
               "where c_component = 'CONSOLE'",0,1,repository_version);

if (repository_version == "")
  {
    repository_version = "1.3";

    return (VOBMGR_K_UPGRADE);
  }

repository_version = "1.3";

return (VOBMGR_K_UPGRADE);
