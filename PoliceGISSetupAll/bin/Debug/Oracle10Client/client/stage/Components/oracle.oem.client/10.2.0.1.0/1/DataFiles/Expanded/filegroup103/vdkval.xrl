/* Copyright (c) Oracle Corporation, 1996, 2000.  All Rights Reserved.    */

/*******************************************************************************
*
*   NAME
*
*       vdkval.xrl
*
*   DESCRIPTION
*
*       Validation script for Oracle Expert
*
*******************************************************************************/

/*******************************************************************************
**  First, check the common validation
*******************************************************************************/

drop_steps = 73;
create_steps = 120;
upgrade_steps = 150;

xp_int_t status = repos::common_validation("EXPERT");
if (status != VOBMGR_K_NOT_FOUND)
  return (status);

status = repos_v1::common_validation("EXPERT");
if (status != VOBMGR_K_NOT_FOUND)
  return (status);

/*******************************************************************************
**  Look for repository control table
*******************************************************************************/

if (!repos::table_exists ("VDK_REP_CONTROL"))
  {
    return (VOBMGR_K_NOT_FOUND);
  }

/*******************************************************************************
**  Check for populated table
*******************************************************************************/

if (!repos::integer ("SELECT COUNT(*) FROM VDK_REP_CONTROL"))
  {
    return (VOBMGR_K_NOT_FOUND);
  }

/*******************************************************************************
**  Get record
*******************************************************************************/

xp_int_t repos;

repos = repos::integer ("SELECT VDK_REPOS_VERSION FROM VDK_REP_CONTROL");

/*******************************************************************************
**  Check for incompatibility
*******************************************************************************/

xp_int_t xp_version = 416;

if (repos < 409)
  {
    repository_version = "1.1";

    return (VOBMGR_K_INCOMPATIBLE);
  }

if (repos > xp_version)
  {
    repository_version = repos;

    return (VOBMGR_K_INCOMPATIBLE);
  }

/*******************************************************************************
**  Check for upgrade
*******************************************************************************/

if (repos < xp_version)
  {
    switch (repos)
      {
        case 408:
          repository_version = "1.1";
          break;

        case 409:
          repository_version = "1.2";
          break;

        case 410:
          repository_version = "1.3";
          break;

        case 411:
          repository_version = "1.4";
          break;

        case 412:
          repository_version = "1.5";
          break;

        case 413:
          repository_version = "1.6";
          break;

        case 414:
          repository_version = "2.0";
          break;

        case 415:
          repository_version = "2.1";
          break;

        case 416:
          repository_version = "2.2";
          break;
      }

    return (VOBMGR_K_UPGRADE);
  }

/*******************************************************************************
**  We must be ok - update the version in smp_rep_version.  This will 
**  effectively disable the old versioning technique.
*******************************************************************************/

repos::update_version("EXPERT");

return (VOBMGR_K_OK);
