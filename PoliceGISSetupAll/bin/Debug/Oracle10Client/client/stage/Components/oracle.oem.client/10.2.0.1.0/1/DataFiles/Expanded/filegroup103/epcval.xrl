/* Copyright (c) Oracle Corporation, 1999 , 2000 All Rights Reserved.    */

/*******************************************************************************
*
*   NAME
*
*       epcval.xrl
*
*   DESCRIPTION
*
*       Validation script for Oracle Trace
*
*******************************************************************************/

/*******************************************************************************
**  First, check the common validation
*******************************************************************************/

drop_steps = 23;
create_steps = 24;
upgrade_steps = 41;

xp_int_t status = repos::common_validation("EPC");
if (status != VOBMGR_K_NOT_FOUND)
  return (status);

/*******************************************************************************
**  Look for old table
*******************************************************************************/

xp_int_t count;
xp_float_t ver;
xp_float_t current = 1.65;

count = repos::table_exists ("EPC_COLLECTION_FACILITIES");
if (count)
  {
    repository_version = "1.2";

    return (VOBMGR_K_UPGRADE);
  }

/*******************************************************************************
**  Check for new table
*******************************************************************************/

count = repos::table_exists ("epc_cli_version");
if (!count)
  {
    return (VOBMGR_K_NOT_FOUND);
  }

/*******************************************************************************
**  Check for valid row in table
*******************************************************************************/

count = repos::integer ("select count(*) from epc_cli_version");
if (!count)
  {
    return (VOBMGR_K_NOT_FOUND);
  }

/*******************************************************************************
**  Check version in table
*******************************************************************************/

repos::select("select version_product from epc_cli_version",0,1,&ver);

if (ver < current)
  {
    repository_version = ver;

    return (VOBMGR_K_UPGRADE);
  }

if (ver > current)
  {
    repository_version = ver;

    return (VOBMGR_K_INCOMPATIBLE);
  }

/*******************************************************************************
**  We must be ok - update the version in smp_rep_version.  This will 
**  effectively disable the old versioning technique.
*******************************************************************************/

repos::update_version("EPC");

return (VOBMGR_K_OK);
