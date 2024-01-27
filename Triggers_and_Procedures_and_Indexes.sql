/*
Trigger for Modification Logging
*/
create or alter  trigger tr_employee_logging
on Employee
after insert, update, delete
as
declare @emp_id int = 0, @operation varchar(1) = 'U', @inserted_data xml, @deleted_data xml  
if (exists(select 1 from inserted) and exists(select 1 from deleted))
  begin
      
    set @operation = 'U' 
    set @inserted_data = (
      select EmpId, FirstName, LastName, Email, Salary, NumOfWorkingHours, ExperienceYear, DeptID, SupervisorID, DateOfBirth from Inserted
      for xml path('Employee'), root('Employees'))
    set @deleted_data = (
      select EmpId, FirstName, LastName, Email, Salary, NumOfWorkingHours, ExperienceYear, DeptID, SupervisorID, DateOfBirth from Deleted
      for xml path('Employee'), root('Employees'))
  end
if (exists(select 1 from inserted) and (not exists(select 1 from deleted)))
  begin
  
    set @operation = 'I'
    set @inserted_data = (
      select EmpId, FirstName, LastName, Email, Salary, NumOfWorkingHours, ExperienceYear, DeptID, SupervisorID, DateOfBirth from Inserted
      for xml path('Employee'), root('Employees'))
  end
if ((not exists(select 1 from inserted)) and exists(select 1 from deleted))
  begin
    
    set @operation = 'D'
    set @deleted_data = (
      select EmpId, FirstName, LastName, Email, Salary, NumOfWorkingHours, ExperienceYear, DeptID, SupervisorID, DateOfBirth from Deleted
      for xml path('Employee'), root('Employees'))
  end
insert into Modifications_Log(tablename, operation, OperationDate, Username, InsertedData, DeletedData)
values ( 'Employee', @operation, GETDATE(), CURRENT_USER, @inserted_data, @deleted_data)

go
/*
Trigger for validating AGE
*/

create or alter trigger tr_only_adult
on Employee
after insert, update
as
begin

  if exists(select * from INSERTED where DateOfBirth > DATEADD(year, -18, GETDATE()))
  begin
    rollback transaction;
    throw 50001, 'Employee must be an adult', 1;
  end
end

go

/*
Trigger for validating EMAIL
*/

create or alter  trigger tr_only_valid_email
on Employee
after insert, update
as
begin
  

  if exists(select * from INSERTED where email not LIKE '_%@_%._%')
  begin
    rollback transaction;
    throw 50001, 'Employee must have a valid email', 1;
  end
end
go

/*
Stored Procedure for Bulk Insertion
*/

create   procedure procBInserting (@Emps utImpEmp readonly) as
begin
  set nocount on

  insert into Employee([FirstName], [LastName], [Email], [Salary], [NumOfWorkingHours], [ExperienceYear], [DateOfBirth])
  select [FirstName], [LastName], [Email], [Salary], [NumOfWorkingHours], [ExperienceYear], [DateOfBirth]                      
  from @Emps                            
end
go

/*
Stored Procedure for Filtration
*/

create   procedure updFiltering
	@FirstName nvarchar(25),
	@LastName nvarchar(25),
	@Email nvarchar(50),
	@Salary decimal(10,2),
	@NumOfWorkingHours int,
	@TotalC int OUT,
	@SortColumn nvarchar(100),
	@SordDesc bit = 0,
	@Page int = 1,
	@pageSize int = 4
as
begin
	declare @sqlFlt nvarchar(1000) = ''
	declare @paramtrs nvarchar(1000) = '@FirstName nvarchar(25), @LastName nvarchar(25), @Email nvarchar(50), @Salary decimal(10,2), @NumOfWorkingHours int, @Rows int, @PageSize int'
	declare @paramtrsTotal nvarchar(1000) = '@totalOUT int OUT, @FirstName nvarchar(25), @LastName nvarchar(25), @Email nvarchar(50), @Salary decimal(10,2), @NumOfWorkingHours int'

	if len(rtrim(ltrim(@FirstName))) > 0
		set @sqlFlt += ' FirstName like @FirstName + ''%'' AND '

	if len(rtrim(ltrim(@LastName))) > 0
		set @sqlFlt += ' LastName like @LastName + ''%'' AND '

	if len(rtrim(ltrim(@Email))) > 0
		set @sqlFlt += ' Email like @Email + ''%'' AND '

	if @Salary is not null
		set @sqlFlt += ' Salary like @Salary AND '

	if @NumOfWorkingHours is not null
		set @sqlFlt += ' NumOfWorkingHours like @NumOfWorkingHours AND '

	if len(@sqlFlt) > 0
		set @sqlFlt = ' WHERE ' + left(@sqlFlt, len(@sqlFlt) - 4)

	------TOTAL
	declare @total nvarchar(160) = N'select @totalOUT = count(*) from Employee ' + @sqlFlt

		exec sp_executesql @total, @paramtrsTotal, @FirstName = @FirstName, @LastName = @LastName, @Email = @Email, @Salary = @Salary, @NumOfWorkingHours = @NumOfWorkingHours, @totalOUT = @TotalC OUT


	-------

	declare @orderBy nvarchar(1000) = ' order by ' + @SortColumn;
	if @SordDesc = 1
	  set @orderBy = @orderBy + ' DESC '

	declare @sql nvarchar (1000) = 'select e.EmpID, e.FirstName, e.LastName, e.Email, e.Salary, e.NumOfWorkingHours, e.ExperienceYear, p.Phone, c.CourseName, uInfo.NumberOfStudents, d.Name
										from Employee e full outer join Lecturer lec
											on e.EmpID = lec.EmpID 
											full outer join UnderG_Lec ul 
											on lec.EmpID = ul.EmpID
											full outer join UndergraduateLevel uInfo
											on ul.CourseID = uInfo.CourseID
											full outer join Course c 
											on e.CourseID = c.CourseId
											full outer join Department d
											on e.DeptID = d.Id
											full outer join Person p
											on p.EmployeeId = e.EmpID '
									+ @sqlFlt 
									+ @orderBy
									+ ' offset @Rows rows fetch next @PageSize rows only'

	
	declare @offset int = (@Page - 1) * @PageSize
	if @offset < 0
		set @offset = 0
	exec sp_executesql @sql, @paramtrs, @FirstName = @FirstName, @LastName = @LastName, @Email = @Email, @Salary = @Salary, @NumOfWorkingHours = @NumOfWorkingHours, @Rows = @offset, @PageSize = @PageSize
end
go

/*
Filtered Employee Exporting as XML 
*/

create   procedure udpEmployeeFilteredExportAsXml(
   @FirstName varchar(25),
   @LastName varchar(25),
   @Email varchar(50),
   @Salary decimal(10,2),
   @NumOfWorkingHours int,
   @xmldata xml out
   ) as
begin
 declare @cnt int
 declare @tab table (
     [EmployeeId] INT          
	,[LastName]   VARCHAR (25)
	,[FirstName]  VARCHAR (25)
	,[Email]      VARCHAR (50)
	,[Salary]	  DECIMAL (10,2)
	,[NumOfWorkingHours] INT
	,[ExperienceYear]  INT
	,[Phone]	  VARCHAR (100)
	,[CourseName] VARCHAR(100)
	,[NumberOfStudent] INT
	,[Name]		  VARCHAR(100)
 )

 insert into @tab
 exec updFiltering 
	@FirstName, 
	@LastName, 
	@Email, 
	@Salary, 
	@NumOfWorkingHours,
	@SortColumn = 'EmpID',
	@PageSize = 9999,
	@TotalC = @cnt OUT

  set @xmldata = (
      select [EmployeeId] 
			  ,[LastName]   
			  ,[FirstName]  
			  ,[Email]      
			  ,[Salary]	  
			  ,[NumOfWorkingHours] 
			  ,[ExperienceYear]  
			  ,[Phone]	  
			  ,[CourseName]
			  ,[NumberOfStudent]
			  ,[Name]		  
	  from @tab
	  for xml path('Employee'), root('Employees')
  )

  select @xmldata
end

go

/*
Filtered Employee Exporting as JSON
*/

create    procedure udpFilteredEmpExportAsJson(
   @FirstName nvarchar(25),
   @LastName nvarchar(25),
   @Email nvarchar(50),
   @Salary decimal(10,2),
   @NumOfWorkingHours int,
   @json varchar(max) out
   ) as
begin
  declare @cnt int
  declare @filteredTab table (
    [EmployeeId] INT          
    ,[FirstName]   nVARCHAR (25)
    ,[LastName]  nVARCHAR (25)
    ,[Email]      nVARCHAR (50)
    ,[Salary]    DECIMAL (10,2)
    ,[NumOfWorkingHours] INT
    ,[ExperienceYear]  INT
    ,[Phone]    nVARCHAR (100)
    ,[CourseName] nVARCHAR(100)
    ,[NumberOfStudent] INT
    ,[Name]      nVARCHAR(100)
  )
 insert into @filteredTab
 exec updFiltering 
  @FirstName, 
  @LastName, 
  @Email, 
  @Salary, 
  @NumOfWorkingHours,
  @PageSize = 9999,
  @SortColumn = 'EmpID',
  @TotalC = @cnt OUT

  set @json = (
      select [EmployeeId] 
    ,[FirstName]   
        ,[LastName]  
        ,[Email]      
        ,[Salary]    
        ,[NumOfWorkingHours] 
        ,[ExperienceYear]  
        ,[Phone]    
        ,[CourseName]
        ,[NumberOfStudent]
        ,[Name]
    from @filteredTab
    for json path, root('Employees')
  ) 

  select @json

end

go

/*
Filtered Employee Exporting as XML
*/
create   procedure udpEmployeeFilteredExportTryingAsCSV(
   @FirstName nvarchar(25),
   @LastName nvarchar(25),
   @Email nvarchar(50),
   @Salary decimal(10,2),
   @NumOfWorkingHours int,
   @csv varchar(max) out
   ) as
begin
  declare @cnt int
  declare @filteredTab table (
    [EmployeeId] INT          
    ,[FirstName]   nVARCHAR (25)
    ,[LastName]  nVARCHAR (25)
    ,[Email]      nVARCHAR (50)
    ,[Salary]    DECIMAL (10,2)
    ,[NumOfWorkingHours] INT
    ,[ExperienceYear]  INT
    ,[Phone]    nVARCHAR (100)
    ,[CourseName] nVARCHAR(100)
    ,[NumberOfStudent] INT
    ,[Name]     nVARCHAR(100)
  )

 insert into @filteredTab
 exec updFiltering 
  @FirstName, 
  @LastName, 
  @Email, 
  @Salary, 
  @NumOfWorkingHours,
  @PageSize = 9999999,
  @SortColumn = 'EmpID',
  @TotalC = @cnt OUT

  set @csv = (
      select string_agg(CONCAT([EmployeeId], ','
        ,[FirstName], ','
        ,[LastName], ','
        ,[Email], ','
        ,[Salary], ','
        ,[NumOfWorkingHours], ','
        ,[ExperienceYear], ','
        ,[Phone], ','
        ,[CourseName], ','
        ,[NumberOfStudent], ','
        ,[Name]), char(13))    
    from @filteredTab)

  select @csv

end
go
go
go
----------------------------------------------------------------------------
----------------------------------------------------------------------------

/*
Query Optimization
*/


/*
To see the Screenshots of more detailed results take a look at our attached report!!! 
*/


/*
One of the best ways to optimize SQL queries is to implement indexes. 
Created indexes are used by SQL itself to retrieve data from table relatively faster if the table contains too many rows. 
However, considering the fact that employee table was used as the main one, it is hardly unlikely that indexing would help optimizing the SQL query. 
As the chosen table’s rows is considered to contain relatively lower number of rows, maximum 100. Yet if the table considered to store more than that, 
then creation of such indexes would be optimal solution to optimize the query. 

However, for the testing purposes we have added 3 indexes to our employee table and tested them by calling our filtering stored procedure. 
The result of the tests was that when calling the procedure and giving it multiple parameters SQL never used the implemented indexes.

Although, when only one parameter is passed to the procedure SQL uses them.

Considering the fact that this very procedure, in most cases, is used with multiple parameters 
we think that the above-mentioned indexes are pointless and have deleted them. 
However, it would be better if we had left them for the future functionalities that might be implemented.
After getting feedback from tutor of using select statements instead of stored procedure the query was used for testing for purposes.

The execution plan of the statement without indices are given in the report as screenshots.

The index we created for this specific statement is a composite one.

However, despite the introduction of the abovementioned index, the SQL server didn’t use it.
We assume that the reason of SQL not using the indices created is, as we mentioned earlier, small number of rows. 
That’s why we think that none of the created indexes makes sense and deleted them.

*/


create index ix_emp_fname on Employee(FirstName)
go
create index ix_emp_lname on Employee(LastName)
go
create index ix_emp_work_hours on Employee(NumOfWorkingHours)
go
create index ix_triple_emp_indeces_main on Employee(FirstName, LastName, NumOfWorkingHours)
go
exec updFiltering @FirstName = 'a',
	@LastName = 'o',
	@Email = null,
	@Salary = null,
	@NumOfWorkingHours = null,
	@TotalC = null,
	@SortColumn = null,
	@SordDesc = 0,
	@Page = 1,
	@pageSize = 999999

go
select e.EmpID, e.FirstName, e.LastName, e.Email, e.Salary, e.NumOfWorkingHours, e.ExperienceYear, p.Phone, c.CourseName, uInfo.NumberOfStudents, d.Name
										from Employee e full outer join Lecturer lec
											on e.EmpID = lec.EmpID 
											full outer join UnderG_Lec ul 
											on lec.EmpID = ul.EmpID
											full outer join UndergraduateLevel uInfo
											on ul.CourseID = uInfo.CourseID
											full outer join Course c 
											on e.CourseID = c.CourseId
											full outer join Department d
											on e.DeptID = d.Id
											full outer join Person p
											on p.EmployeeId = e.EmpID
											where e.FirstName like 'al%' and e.LastName like '%o%' and e.NumOfWorkingHours =10

go




