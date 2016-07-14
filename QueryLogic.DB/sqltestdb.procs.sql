create procedure gettestdata @test_key int
as begin
   if @test_key > 0 begin
      select sqltestdata.sqltest_int32,
             sqltestdata.sqltest_string,
             sqltestdata.sqltest_decimal,
             sqltestdata.sqltest_datetime
        from sqltestdata
       where sqltestdata.sqltest_int32 = @test_key
    end
    else
       select sqltestdata.sqltest_int32,
             sqltestdata.sqltest_string,
             sqltestdata.sqltest_decimal,
             sqltestdata.sqltest_datetime
        from sqltestdata
end
go

create procedure addtestentry @test_string   varchar(350),
                              @test_decimal  decimal(15,3),
                              @test_datetime datetime
as begin
   insert into sqltestdata
      ( sqltest_string, sqltest_decimal, sqltest_datetime ) 
   values
      ( @test_string, @test_decimal, @test_datetime )
end
go

create procedure updatetestentry @test_key int,
                                 @test_string   varchar(350),
                                 @test_decimal  decimal(15,3),
                                 @test_datetime datetime
as begin
   update sqltestdata
      set sqltest_string = @test_string,
          sqltest_decimal = @test_decimal,
          sqltest_datetime = @test_datetime
    where sqltestdata.sqltest_int32 = @test_key
end
go

create procedure savetestdata @id int = null,
                              @string varchar(350),
                              @decimal decimal(15,3),
                              @datetime datetime
as begin
   if @id is null begin
      insert into sqltestdata
         ( sqltest_string, sqltest_decimal, sqltest_datetime )
      values
         ( @string, @decimal, @datetime )
   end
   else begin
      update sqltestdata
         set sqltest_string = @string,
             sqltest_decimal = @decimal,
             sqltest_datetime = @datetime
       where sqltestdata.sqltest_int32 = @id
    end
end
go

create procedure deletetestentry @test_key int
as begin
   delete from sqltestdata
    where sqltestdata.sqltest_int32 = @test_key
end
go

create procedure getmaxid
as begin
   select max(sqltestdata.sqltest_int32) as max_id
     from sqltestdata
end
go

create procedure getsqloutput @input int,
                              @output int output
as begin
   select @output = @input
end
go

create procedure addbinarydata @bytecode binary(100)
as begin
   insert into sqlbinarytest ( sqlbinarytest_bytecode ) values ( @bytecode )
end
go

create procedure getbinarydata @binaryid int
as begin
   select sqlbinarytest.sqlbinarytest_id,
          sqlbinarytest.sqlbinarytest_bytecode
     from sqlbinarytest
    where sqlbinarytest.sqlbinarytest_id = @binaryid
end
go

create procedure getmanifestedsqltestdata @getaddl  int,
                                          @getmore  int
as begin
   select mstd.sqltestdata_id,
          mstd.sqltestdata_value
     from manifestedsqltestdata mstd
	  
   if @getaddl > 0 begin
	  select amstd.sqltestdata_id,
	         amstd.addlsqltestdata_id,
             amstd.addlsqltestdata_value    
	    from addlmanifestedsqltestdata amstd
		join manifestedsqltestdata mstd on mstd.sqltestdata_id = amstd.sqltestdata_id
   end

   if @getmore > 0 begin
      select mmstd.sqltestdata_id,
	         mmstd.moresqltestdata_id,
             mmstd.moresqltestdata_value
	    from moremanifestedsqltestdata mmstd		
		join manifestedsqltestdata mstd on mstd.sqltestdata_id = mmstd.sqltestdata_id
   end
end
go