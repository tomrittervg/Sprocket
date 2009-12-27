create procedure simpletestproc_prc
(  
 @companyId int,  
 @objectid float,  
 @personname varchar(255)  
) as  
  
select @companyId, @objectid, @personname