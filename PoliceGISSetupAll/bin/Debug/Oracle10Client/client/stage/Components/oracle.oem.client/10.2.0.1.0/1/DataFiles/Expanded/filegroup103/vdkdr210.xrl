/* Copyright (c) Oracle Corporation, 1994, 1986.  All Rights Reserved.    */

/*******************************************************************************
*
*   NAME
*
*       vdkdr210.xrl
*
*   DESCRIPTION
*
*       Deletes 2.1.0 repository items for the Expert.
*
*   NOTES
*
*
*
*******************************************************************************/

/*******************************************************************************
**  Drop all objects
*******************************************************************************/

xp_str_t buf,name,type;
xp_int_t s1;

s1 = repos::open ("select object_type,object_name from user_objects "
                  "where object_name like 'VDK_%' "
                  "and object_type <> 'INDEX' and object_type <> 'CLUSTER'");

while (repos::fetch (&s1, type, name))
  {
    sprintf(buf,"DROP %1 %2", type,name);

    repos::execute_ignore (buf);
    repos::work_in_progress ();
  }

repos::close (&s1);

s1 = repos::open ("select object_type,object_name from user_objects "
                  "where object_name like 'VDK_%' "
                  "and object_type = 'CLUSTER'");

while (repos::fetch (&s1, type, name))
  {
    sprintf(buf,"DROP %1 %2", type,name);

    repos::execute_ignore (buf);
    repos::work_in_progress ();
  }

repos::close (&s1);

