/* Copyright (c) Oracle Corporation, 1996.  All Rights Reserved.    */

/*******************************************************************************
*
*   NAME
*
*       epcfmtv.xrl
*
*   DESCRIPTION
*
*       Validation script for Oracle Trace formatted databases
*
*******************************************************************************/

/*******************************************************************************
**  First, check the common validation
*******************************************************************************/

drop_steps = 6;
create_steps = 8;
upgrade_steps = 13;

xp_int_t status = repos::common_validation("EPCFMT");
if (status != VOBMGR_K_NOT_FOUND)
  return (status);

/*******************************************************************************
**  Look for old table
*******************************************************************************/

xp_int_t count;

count = repos::column_exists ("COLLECTION", "COLLECTION_ID");
if (count)
  {
    count = repos::column_exists("EPC_COLLECTION", "COLLECTION_ID");
    if (count)
      {
        repository_version = "1.4";
        return (VOBMGR_K_OK);
      }
    
    repository_version = "1.2";
    return (VOBMGR_K_UPGRADE);
  }

/*******************************************************************************
**  Check for new table
*******************************************************************************/

count = repos::column_exists ("EPC_COLLECTION", "COLLECTION_ID");
if (!count)
  {
    repository_version = "1.4";

    return (VOBMGR_K_NOT_FOUND);
  }

/*******************************************************************************
**  We must be ok 
*******************************************************************************/

repos::update_version("EPCFMT");

return (VOBMGR_K_OK);
