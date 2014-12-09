/* Copyright (c) Oracle Corporation, 1994, 1986.  All Rights Reserved.    */

/*******************************************************************************
*
*   NAME
*
*       epcf140.xrl
*
*   DESCRIPTION
*
*       Updates repository version to the current product version.
*
*   NOTES
*
*
*
*******************************************************************************/


/* UPDATE THE SEQUENCE */
xp_int_t s1;
xp_int_t max_id;

s1 = repos::open("select collection_id.nextval from collection");
repos::fetch(&s1, max_id);
repos::close(&s1);
repos::execute(concat("create sequence epc_collection_id increment by 1 start with ",max_id));
repos::execute("drop sequence collection_id");

repos::work_in_progress();

/* MOVE DATA TO NEW TABLES */
repos::execute("insert into epc_collection select * from collection");
repos::execute("insert into epc_process select * from process");
repos::execute("insert into epc_facility select * from facility");

repos::work_in_progress();

repos::execute("insert into epc_event select * from event");
repos::execute("insert into epc_item select * from item");
repos::execute("insert into epc_event_item select * from event_item");
repos::execute("insert into epc_facility_registration select * from facility_registration");

repos::commit();   
