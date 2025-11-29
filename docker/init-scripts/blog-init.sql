create extension if not exists "uuid-ossp";

create table if not exists Posts (
    Id uuid primary key,
    Title varchar(200) not null,
    Content text not null,
    UserId uuid not null,
    CreatedAt timestamp default now(),
    UpdatedAt timestamp default null
);

create table if not exists Comments (
    Id uuid primary key,
    PostId uuid references Posts(Id) on delete cascade,
    UserId uuid not null,
    Content text not null,
    CreatedAt timestamp default now()
);

create table if not exists Tags (
    Id uuid primary key,
    Name varchar(100) unique not null
);

create table if not exists Posts_Tags (
    PostId uuid references Posts(Id) on delete cascade,
    TagId uuid references Tags(Id) on delete cascade,
    primary key (PostId, TagId)
);

create table if not exists Posts_Users (
    PostId uuid references Posts(Id) on delete cascade,
    UserId uuid not null,
    primary key (PostId, UserId)
);

create index if not exists posts_userid_index on Posts (UserId);
create index if not exists comments_postid_index on Comments (PostId);
create index if not exists tag_name_index on Tags using hash (Name);
create index if not exists post_tags_postid_index on Posts_Tags (PostId);
create index if not exists post_tags_tagid_index on Posts_Tags (TagId);
create index if not exists post_users_postid_index on Posts_Users (PostId);
create index if not exists post_users_userid_index on Posts_Users (UserId);

create or replace function get_post_by_id(
	post_id uuid
) returns table (
	id uuid,
	title varchar(200),
	content text,
	user_id uuid,
	created_at timestamp,
	updated_at timestamp
) as $$
begin
	return query
	select p.Id, p.Title, p.Content, p.UserId, p.CreatedAt, p.UpdatedAt from Posts p
	where p.Id = post_id;
end;
$$ language plpgsql;

create or replace function get_posts_by_tags(
	variadic post_tags varchar(100)[]
) returns table (
	id uuid,
	title varchar(200),
	content text,
	user_id uuid,
	created_at timestamp,
	updated_at timestamp
) as $$
begin
	return query
    select distinct p.Id, p.Title, p.Content, p.UserId, p.CreatedAt, p.UpdatedAt from Posts p
    join Posts_Tags pt on p.Id = pt.PostId
    join Tags t on pt.TagId = t.Id
    where t.Name = any(post_tags)
    order by p.CreatedAt desc;
end;
$$ language plpgsql;

create or replace function get_user_posts(
	check_user_id uuid
) returns table (
	id uuid,
	title varchar(200),
	content text,
	user_id uuid,
	created_at timestamp,
	updated_at timestamp
) as $$
begin
	return query
	select p.Id, p.Title, p.Content, p.UserId, p.CreatedAt, p.UpdatedAt from Posts p
	where p.UserId = check_user_id;
end;
$$ language plpgsql;

create or replace function create_post(
	p_id uuid,
	p_title varchar(200),
	p_content text,
	p_user_id uuid,
	p_created_at timestamp
) returns uuid as $$
begin
	insert into Posts (Id, Title, Content, UserId, CreatedAt, UpdatedAt)
	values (p_id, p_title, p_content, p_user_id, p_created_at, null);
	
	return p_id;
end;
$$ language plpgsql;

create or replace function update_post(
	p_id uuid,
	p_title varchar(200),
	p_content text,
	p_updated_at timestamp
) returns uuid as $$
begin
	update Posts p
	set p.Title = p_title, p.Content = p_content, p.UpdatedAt = p_updated_at
	where p.Id = p_id;
	
	return p_id;
end;
$$ language plpgsql;

create or replace function delete_post(
	id uuid
) returns uuid as $$
begin
	delete from Posts p
	where p.Id = id;
	
	return id;
end;
$$ language plpgsql;

create or replace function get_post_comments(
	check_post_id uuid
) returns table (
	id uuid,
	post_id uuid,
	user_id uuid,
	content text,
	created_at timestamp
) as $$
begin
	return query
	select c.Id, c.PostId, c.UserId, c.Content, c.CreatedAt from Comments c
	where check_post_id = c.PostId;
end;
$$ language plpgsql;

create or replace function create_comment(
	p_id uuid,
	p_post_id uuid,
	p_user_id uuid,
	p_content text,
	p_created_at timestamp
) returns uuid as $$
begin
	insert into Comments (Id, PostId, UserId, Content, CreatedAt)
	values (p_id, p_post_id, p_user_id, p_content, p_created_at);
	
	return p_id;
end;
$$ language plpgsql;

create or replace function delete_comment(
	id uuid
) returns uuid as $$
begin
	delete from Comments c
	where id = c.Id;
	
	return id;
end;
$$ language plpgsql;

create or replace function get_tag_by_name(
	check_name varchar(100)
) returns table (
	id uuid,
	name varchar(100)
) as $$
begin
	return query
	select t.Id, t.Name from Tags t
	where check_name = t.Name;
end;
$$ language plpgsql;

create or replace function create_tag(
	p_id uuid,
	p_name varchar(100)
) returns uuid as $$
begin
	insert into Tags (Id, Name)
	values (p_id, p_name);
	
	return p_id;
end;
$$ language plpgsql;

create or replace function is_user_like_post(
	user_id uuid,
	post_id uuid
) returns boolean as $$
begin
	return exists(
        select pu.UserId from Posts_Users pu
        where user_id = pu.UserId and post_id = pu.PostId
    );
end;
$$ language plpgsql;

create or replace function change_like_post_by_user(
	user_id uuid,
	post_id uuid
) returns boolean as $$
begin
	if is_user_like_post(user_id, post_id) then
		delete from Posts_Users pu
		where user_id = pu.UserId and post_id = pu.PostId;

		return false;
	else
		insert into Posts_Users (PostId, UserId)
		values (like_post_by_user.*);

		return true;
	end if;
end;
$$ language plpgsql;







