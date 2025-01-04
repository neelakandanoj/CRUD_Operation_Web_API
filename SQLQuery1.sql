drop table if exists Products

CREATE TABLE products(
id INT NOT NULL PRIMARY KEY iDENTITY,
name NVarchar(100) not null,
brand NVarchar(100) not null,
category NVarchar(100) not null,
price decimal(16,2) not null,
description NVarchar(max) not null,
created_at datetime2 not null default current_timestamp
)

select * from products(nolock)