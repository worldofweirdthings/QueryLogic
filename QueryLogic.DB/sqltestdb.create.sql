create table sqltestdata
(
   sqltest_int32    int identity primary key,
   sqltest_string   varchar(350),
   sqltest_decimal  decimal(15,3),
   sqltest_datetime datetime
)

create table sqlbinarytest
(
   sqlbinarytest_id int identity primary key,
   sqlbinarytest_bytecode binary(100)
)

create table manifestedsqltestdata
(
   sqltestdata_id     int identity primary key,
   sqltestdata_value  varchar (250)
)

create table addlmanifestedsqltestdata
(
   addlsqltestdata_id     int identity primary key,
   addlsqltestdata_value  varchar (250),
   sqltestdata_id         int foreign key references manifestedsqltestdata(sqltestdata_id)
)

create table moremanifestedsqltestdata
(
   moresqltestdata_id     int identity primary key,
   moresqltestdata_value  varchar (250),
   sqltestdata_id         int foreign key references manifestedsqltestdata(sqltestdata_id)
)

insert into sqltestdata ( sqltest_string, sqltest_decimal, sqltest_datetime ) values ( 'sql test string', 4.3, current_timestamp )
insert into sqlbinarytest ( sqlbinarytest_bytecode ) values ( 0xFF )
insert into manifestedsqltestdata ( sqltestdata_value ) values ( 'sqltestdata' )
insert into addlmanifestedsqltestdata ( addlsqltestdata_value, sqltestdata_id ) values ( 'addlsqltestdata', 1 )
insert into moremanifestedsqltestdata ( moresqltestdata_value, sqltestdata_id ) values ( 'moresqltestdata', 1 )