/* Copyright (c) Oracle Corporation, 1996, 2000.  All Rights Reserved.    */

/*******************************************************************************
*
*   NAME
*
*       vobutil.xrl
*
*   DESCRIPTION
*
*       Repository manager utility scripts.  
*
*   NOTES
*
*******************************************************************************/

/*******************************************************************************
*
*   Routine:
*
*       repos::common_validation
*
*   Description:
*
*       A common validation routine that checks either the V1 version
*       table or the V2 version depending on the user option.  It can
*       be used by each sub-component to manage a single versioning
*       scheme.
*
*   Formal parameters:
*
*       component - caller's component name (V1 & V2 component names are the same)
*
*   Returns:
*
*       VOB status
*
*******************************************************************************/

replace procedure repos::common_validation (component)

  {
    xp_str_t c1 = component;
    xp_str_t c2 = component;

    return (repos::common_validation2(c1, c2));
  }


/*******************************************************************************
*
*   Routine:
*
*       repos::common_validation2
*
*   Description:
*
*       A common validation routine that checks either the V1 version
*       table or the V2 version depending on the user option.  It can
*       be used by each sub-component to manage a single versioning
*       scheme.
*
*   Formal parameters:
*
*       v1_name   - caller's v1 component name
*       v2_name   - caller's v2 component name
*
*   Returns:
*
*       VOB status
*
*******************************************************************************/

replace procedure repos::common_validation2 (v1_name, v2_name)

  {
    xp_int_t tmp;

    if (vers_tbl_loc == 1)
      tmp = repos_v1::common_validation(v1_name);
    else if (vers_tbl_loc == 2)
      tmp = repos_v2::common_validation(v2_name);
    else
      tmp = VOBMGR_K_FATAL;

    return (tmp);
  }


/*******************************************************************************
*
*   Routine:
*
*       repos_v1::common_validation
*
*   Description:
*
*       A common validation routine that checks the SMP version table.  It
*       can be used by each sub-component to manage a single versioning
*       scheme.
*
*   Formal parameters:
*
*       component   - caller's component name
*
*   Returns:
*
*       VOB status
*
*   NOTE:
*       ** For V1 repository use only. **
*
*******************************************************************************/

replace procedure repos_v1::common_validation (component)

  {
    repository_version = "";

    /***************************************************************************
    **  Look for repository control table.
    ***************************************************************************/

    if (!repos::table_exists ("smp_rep_version"))
      {
        return (VOBMGR_K_NOT_FOUND);
      }

    /***************************************************************************
    **  Look for column.  If not found, add the column and set any existing
    **  entries as 'CONSOLE' entries.
    ***************************************************************************/

    if (!repos::column_exists ("smp_rep_version", "c_component"))
      {
        repos::execute("alter table smp_rep_version "
                       "add (c_component varchar(255))");

        repos::execute("update smp_rep_version set c_component = 'CONSOLE'");

        repos::commit();
      }

    /***************************************************************************
    **  Check for populated table
    ***************************************************************************/

    if (!repos::integer ("select count(*) from smp_rep_version"))
      {
        return (VOBMGR_K_NOT_FOUND);
      }

    /***************************************************************************
    **  Get record
    ***************************************************************************/

    repos::select("select c_current_version from smp_rep_version where "
                  "c_component = :1", 
                  1,1, component, repository_version);

    if (repository_version == "")
      {
        return (VOBMGR_K_NOT_FOUND);
      }

    /***************************************************************************
    **  Check for incompatibility
    ***************************************************************************/
    
    xp_int_t diff = compare_version (repository_version, current_version);

    if (diff < 0)
      {
        return (VOBMGR_K_UPGRADE);
      }

    if (diff > 0)
      {
        return (VOBMGR_K_INCOMPATIBLE);
      }

    /***************************************************************************
    **  We must be ok
    ***************************************************************************/

    return (VOBMGR_K_OK);
  }


/*******************************************************************************
*
*   Routine:
*
*       repos_v2::common_validation
*
*   Description:
*
*       A common validation routine that checks the SMP version table in a
*       V2 schema.  It can be used by each sub-component to manage a 
*       single versioning scheme.  This call is primarily used in a V2 VALIDATE
*       environment.
*
*   Formal parameters:
*
*       component   - caller's component name
*
*   Returns:
*
*       VOB status
*
*   NOTE:
*       ** For V2 repository use only. **
*
*******************************************************************************/

replace procedure repos_v2::common_validation (component)

  {
    repository_version = "";

    /***************************************************************************
    **  Look for repository control table.
    ***************************************************************************/

    if (!repos::table_exists ("smp_vds_repos_version"))
      {
        repos::execute ("create table SMP_VDS_REPOS_VERSION "
                        "(app_name         VARCHAR2(512) PRIMARY KEY,"
                        " version          NUMBER NOT NULL,"
                        " upd_in_progress  NUMBER NOT NULL)");
      }

    /***************************************************************************
    **  Check for populated table
    ***************************************************************************/

    if (!repos::integer ("select count(*) from smp_vds_repos_version "
                          "where app_name = :1", component))
      {
        /***********************************************************************
        **  Special case "EXPERT"
        ***********************************************************************/

        if (component == "EXPERT")
          {
            return (repos_v1::common_validation (component));
          }

        return (VOBMGR_K_NOT_FOUND);
      }

    /***************************************************************************
    **  Get record
    ***************************************************************************/

    int version;

    repos::select("select version from smp_vds_repos_version where "
                   "app_name = :1", 
                  1,1, component, &version);

    switch (version)
      {
        case 1:
          repository_version = "2.0";
          break;
        
        case 2:
          repository_version = "2.1";
          break;

        case 3:
          repository_version = "2.2";
          break;
      }

    if (repository_version == "")
      {
        return (VOBMGR_K_NOT_FOUND);
      }

    /***************************************************************************
    **  Check for incompatibility
    ***************************************************************************/

    xp_int_t diff = compare_version (repository_version, current_version);

    if (diff < 0)
      {
        return (VOBMGR_K_UPGRADE);
      }

    if (diff > 0)
      {
        return (VOBMGR_K_INCOMPATIBLE);
      }

    /***************************************************************************
    **  We must be ok
    ***************************************************************************/

    return (VOBMGR_K_OK);
  }

/*******************************************************************************
*
*   Routine:
*
*       repos::update_version
*
*   Description:
*
*       A common update routine that sets the current version for a 
*       component in the V1 or V2 version table depending on the user
*       option and current action.
*
*   Formal parameters:
*
*       component   - caller's component name
*
*   Returns:
*
*       VOB status
*
*   NOTE:
*       ** For V1 repository use only. **
*
*******************************************************************************/

replace procedure repos::update_version (component)

  {
    if (vers_tbl_loc == 1)
      return (repos_v1::update_version(component));
    else if (vers_tbl_loc == 2)
      return (repos_v2::update_version(component));
    else if (vers_tbl_loc == 3)
      return (target::update_version(component));

  }


/*******************************************************************************
*
*   Routine:
*
*       repos_v1::update_version
*
*   Description:
*
*       A common update routine that sets the current version for a 
*       component.
*
*   Formal parameters:
*
*       component   - caller's component name
*
*   Returns:
*
*       VOB status
*
*   NOTE:
*       ** For V1 repository use only. **
*
*******************************************************************************/

replace procedure repos_v1::update_version (component)

  {
    repository_version = current_version;

    /***************************************************************************
    **  Look for repository control table.  If it doesn't exist,
    **  create it before trying to insert into it.
    ***************************************************************************/

    if (!repos::table_exists ("smp_rep_version"))
      {
        repos::execute ("create table smp_rep_version "
                        "(c_component varchar(255),"
                        " c_current_version varchar(255),"
                        " c_unused varchar(255))");
      }

    /***************************************************************************
    **  Look for column.  If not found, add the column and set any existing
    **  entries as 'CONSOLE' entries.
    ***************************************************************************/

    if (!repos::column_exists ("smp_rep_version", "c_component"))
      {
        repos::execute("alter table smp_rep_version "
                       "add (c_component varchar(255))");

        repos::execute("update smp_rep_version set c_component = 'CONSOLE'");
      }

    /***************************************************************************
    **  Update the version.  Insert it if it's not there
    ***************************************************************************/

    if (!repos::integer ("select count(*) from smp_rep_version "
                         "where c_component = :1", component))
      {
        repos::execute("insert into smp_rep_version (c_component, "
                       "c_current_version, c_unused) "
                       "values (:1,:2,'')", component, current_version);
      }
    else
      {
        repos::execute("update smp_rep_version "
                       "set c_current_version = :2 "
                       "where c_component = :1", 
                       component, current_version);
      }

    /***************************************************************************
    **  Delete the obsolete CONTROL version record if it exists
    **  (V1.4 upgrade).  Ignore errors.
    ***************************************************************************/

    repos::execute_ignore("delete from smp_rep_version "
                          "where c_component = 'CONTROL'"); 

    repos::commit();
  }


/*******************************************************************************
*
*   Routine:
*
*       repos_v2::update_version
*
*   Description:
*
*       A common update routine that sets the current version for a 
*       component.
*
*   Formal parameters:
*
*       component   - caller's component name
*
*   Returns:
*
*       VOB status
*
*   NOTE:
*       ** For V2 repository use only. **
*
*******************************************************************************/

replace procedure repos_v2::update_version (component)

  {
    repository_version = current_version;
    
    int version;

    switch (repository_version)
      {
        case "2.0":
          version = 1;
          break;

        case "2.1":
          version = 2;
          break;

        case "2.2":
          version = 3;
          break;

        default:
          version = 3;
          break;
      }

    /***************************************************************************
    **  Look for repository control table.  If it doesn't exist,
    **  return the appropriate status.
    ***************************************************************************/

    if (!repos::table_exists ("smp_vds_repos_version"))
      {
        repos::execute ("create table SMP_VDS_REPOS_VERSION "
                        "(app_name         VARCHAR2(512) PRIMARY KEY,"
                        " version          NUMBER NOT NULL,"
                        " upd_in_progress  NUMBER NOT NULL)");
      }

    /***************************************************************************
    **  Update the version.  Insert it if it's not there
    **  NOTE: Hardcode V2.x version # to "1" for the first release.
    ***************************************************************************/

    if (!repos::integer ("select count(*) from smp_vds_repos_version "
                          "where app_name = :1", component))
      {
        repos::execute("insert into smp_vds_repos_version (app_name, version, "
                        "upd_in_progress) values (:1, :2, 0)", 
                       component,&version);
      }
    else
      {
        repos::execute("update smp_vds_repos_version set version = :2 where "
                        "app_name = :1", component,&version);
      }

    repos::commit();
  }


/*******************************************************************************
*
*   Routine:
*
*       repos::delete_version
*
*   Description:
*
*       A common delete routine that removes the current version for a 
*       component.
*
*   Formal parameters:
*
*       component   - caller's component name
*
*   Returns:
*
*       VOB status
*
*   NOTE:
*       ** For V2 repository use only. **
*
*******************************************************************************/

replace procedure repos::delete_version (component)

  {
    if (vers_tbl_loc == 2)
      return (repos_v2::delete_version(component));
    else if (vers_tbl_loc == 3)
      return (target::delete_version(component));
  }


/*******************************************************************************
*
*   Routine:
*
*       repos_v2::delete_version
*
*   Description:
*
*       A common delete routine that removes the current version for a 
*       component.
*
*   Formal parameters:
*
*       component   - caller's component name
*
*   Returns:
*
*       VOB status
*
*   NOTE:
*       ** For V2 repository use only. **
*
*******************************************************************************/

replace procedure repos_v2::delete_version (component)

  {
    if (!repos::table_exists ("smp_vds_repos_version"))
      return;

    repos::execute_ignore("delete from smp_vds_repos_version where app_name = :1", 
                     component);

    if (!repos::integer("select count(*) from smp_vds_repos_version "))
      {
        repos::execute_ignore("drop table smp_vds_repos_version");
      }

    repos::commit();
  }




/*******************************************************************************
*
*   Routine:
*
*       target::common_validation
*
*   Description:
*
*       A common validation routine that checks the SMP version table in a
*       target schema.  It can be used by each sub-component to manage a 
*       single versioning scheme.  This call is primarily used in a 
*       multiple connection transaction where repos::common_validation services the
*       source database and target::common_validation services the target
*       database.
*
*   Formal parameters:
*
*       component   - caller's component name
*
*   Returns:
*
*       VOB status
*
*   NOTE:
*       ** For V2 repository use only. **
*
*******************************************************************************/

replace procedure target::common_validation (component)

  {
    repository_version = "";

    /***************************************************************************
    **  Look for repository control table.
    ***************************************************************************/

    if (!target::table_exists ("smp_vds_repos_version"))
      {
        return (VOBMGR_K_NOT_FOUND);
      }

    /***************************************************************************
    **  Check for populated table
    ***************************************************************************/

    if (!target::integer ("select count(*) from smp_vds_repos_version "
                          "where app_name = :1", component))
      {
        return (VOBMGR_K_NOT_FOUND);
      }

    /***************************************************************************
    **  Get record
    ***************************************************************************/

    int version;

    target::select("select version from smp_vds_repos_version where "
                   "app_name = :1",1,1, component, &version);

    switch (version)
      {
        case 1:
          repository_version = "2.0";
          break;

        case 2:
          repository_version = "2.1";
          break;

        case 3:
          repository_version = "2.2";
          break;
      }
    
    if (repository_version == "")
      {
        return (VOBMGR_K_NOT_FOUND);
      }

    /***************************************************************************
    **  Check for incompatibility
    ***************************************************************************/

    xp_int_t diff = compare_version (repository_version, current_version);

    if (diff < 0)
      {
        return (VOBMGR_K_UPGRADE);
      }

    if (diff > 0)
      {
        return (VOBMGR_K_INCOMPATIBLE);
      }

    /***************************************************************************
    **  We must be ok
    ***************************************************************************/

    return (VOBMGR_K_OK);
  }


/*******************************************************************************
*
*   Routine:
*
*       target::update_version
*
*   Description:
*
*       A common update routine that sets the current version for a 
*       component.
*
*   Formal parameters:
*
*       component   - caller's component name
*
*   Returns:
*
*       VOB status
*
*   NOTE:
*       ** For V2 repository use only. **
*
*******************************************************************************/

replace procedure target::update_version (component)

  {
    repository_version = current_version;

    /***************************************************************************
    **  Look for repository control table.  If it doesn't exist,
    **  return the appropriate status.
    ***************************************************************************/

    if (!target::table_exists ("smp_vds_repos_version"))
      {
        return (VOBMGR_K_NOT_FOUND);
      }

    /***************************************************************************
    **  Update the version.  Insert it if it's not there
    **  NOTE: Hardcode V2.x version # to "1" fro the first release.
    ***************************************************************************/

    int version;

    switch (repository_version)
      {
        case "2.0":
          version = 1;
          break;

        case "2.1":
          version = 2;
          break;

        case "2.2":
          version = 3;
          break;

        default:
          version = 3;
          break;
      }

    if (!target::integer ("select count(*) from smp_vds_repos_version "
                          "where app_name = :1", component))
      {
        target::execute("insert into smp_vds_repos_version (app_name, version, "
                        "upd_in_progress) values (:1, :2, 0)", 
                        component,version);
      }
    else
      {
        target::execute("update smp_vds_repos_version set version = :2 where "
                        "app_name = :1", component,&version);
      }

    target::commit();
  }


/*******************************************************************************
*
*   Routine:
*
*       target::delete_version
*
*   Description:
*
*       A common delete routine that removes the current version for a 
*       component.
*
*   Formal parameters:
*
*       component   - caller's component name
*
*   Returns:
*
*       VOB status
*
*   NOTE:
*       ** For V2 repository use only. **
*
*******************************************************************************/

replace procedure target::delete_version (component)

  {
    /***************************************************************************
    **  Delete the version record.  If that leaves no components, or
    **  just the CONTROL component, then drop the table.  It'll be
    **  recreated if and when a component needs it.
    ***************************************************************************/

    target::execute("delete from smp_vds_repos_version where app_name = :1", 
                     component);

    target::commit();
  }
