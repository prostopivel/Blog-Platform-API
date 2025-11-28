create extension if not exists "uuid-ossp";

create table if not exists Users (
	Id uuid primary key,
	Username varchar(100) not null,
	Email varchar(100) unique not null,
	PasswordHash varchar(100) not null
);

create index if not exists user_email_index on Users (Email);

create or replace function get_user_by_id(
	user_id uuid
) returns table (
	id uuid,
	username varchar(100),
	email varchar(100),
	password_hash varchar(100)
) as $$
begin
	return query
	select u.Id, u.Username, u.Email, u.PasswordHash from Users u
	where u.Id = user_id;
end;
$$ language plpgsql;

create or replace function get_user_by_email(
	user_email varchar(100)
) returns table (
	id uuid,
	username varchar(100),
	email varchar(100),
	password_hash varchar(100)
) as $$
begin
	return query
	select u.Id, u.Username, u.Email, u.PasswordHash from Users u
	where u.Email = user_email;
end;
$$ language plpgsql;

create or replace function create_user(
	p_id uuid,
	p_username varchar(100),
	p_email varchar(100),
	p_password_hash varchar(100)
) returns uuid as $$
begin
	insert into Users (Id, Username, Email, PasswordHash)
	values (p_id, p_username, p_email, p_password_hash);
	
	return p_id;
end;
$$ language plpgsql;

create or replace function exists_user_by_email(
    user_email varchar(100)
) returns boolean as $$
begin
    return exists(
        select u.Id, u.Username, u.Email, u.PasswordHash 
        from Users u
        where u.Email = user_email
    );
end;
$$ language plpgsql;












