/* Copyright (c) Oracle Corporation, 1994, 1986.  All Rights Reserved.    */

/*******************************************************************************
*
*   NAME
*
*       xpodr.xrl
*
*   DESCRIPTION
*
*       Deletes repository version for the current product.
*
*   NOTES
*
*
*
*******************************************************************************/

/*******************************************************************************
**  Drop all indexes
*******************************************************************************/

xp_str_t buf,buf1;
xp_int_t s1;

s1 = repos::open ("select index_name from user_indexes "
                  "where index_name like 'XP_%'");

while (repos::fetch (&s1, buf1))
  {
    sprintf(buf,"DROP INDEX %1", buf1);

    repos::execute_ignore (buf);
  }

repos::close (&s1);

repos::work_in_progress ();

