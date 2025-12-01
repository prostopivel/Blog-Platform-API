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
	p_id uuid
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
	where p_id = p.Id;
end;
$$ language plpgsql;

create or replace function get_posts_by_tags(
	limit_count integer default 10,
	offset_count integer default 0,
	variadic post_tags varchar(100)[] default null
) returns table (
	id uuid,
	title varchar(200),
	content text,
	user_id uuid,
	created_at timestamp,
	updated_at timestamp,
	total_count bigint
) as $$
begin
	return query
	select distinct p.Id, p.Title, p.Content, p.UserId, p.CreatedAt, p.UpdatedAt,
	count(*) over() as total_count from Posts p
	join Posts_Tags pt on p.Id = pt.PostId
	join Tags t on pt.TagId = t.Id
	where t.Name = any(post_tags)
	order by p.CreatedAt desc
	limit limit_count
	offset offset_count;
end;
$$ language plpgsql;

create or replace function get_user_posts(
	check_user_id uuid,
	limit_count integer default 10,
	offset_count integer default 0
) returns table (
	id uuid,
	title varchar(200),
	content text,
	user_id uuid,
	created_at timestamp,
	updated_at timestamp,
	total_count bigint
) as $$
begin
	return query
	select p.Id, p.Title, p.Content, p.UserId, p.CreatedAt, p.UpdatedAt,
	count(p.Id) over() as total_count from Posts p
	where p.UserId = check_user_id
	order by p.CreatedAt desc
	limit limit_count
	offset offset_count;
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
	update Posts
	set Title = p_title, Content = p_content, UpdatedAt = p_updated_at
	where Id = p_id;
	
	return p_id;
end;
$$ language plpgsql;

create or replace function delete_post(
	p_id uuid
) returns uuid as $$
begin
	delete from Posts p
	where p_id = p.Id;
	
	return p_id;
end;
$$ language plpgsql;

create or replace function exists_post(
	p_id uuid
) returns boolean as $$
begin
	return exists(
		select p.Id from Posts p
		where p_id = p.Id
	);
end;
$$ language plpgsql;

create or replace function is_user_post(
	p_id uuid,
	p_user_id uuid
) returns boolean as $$
begin
	return exists(
		select p.Id from Posts p
		where p_id = p.Id and p_user_id = p.UserId
	);
end;
$$ language plpgsql;

create or replace function get_comment_by_id(
	p_id uuid
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
	where p_id = c.Id;
end;
$$ language plpgsql;

create or replace function get_post_comments(
	check_post_id uuid,
	limit_count integer default 20,
	offset_count integer default 0
) returns table (
	id uuid,
	post_id uuid,
	user_id uuid,
	content text,
	created_at timestamp,
	total_count bigint
) as $$
begin
	return query
	select c.Id, c.PostId, c.UserId, c.Content, c.CreatedAt,
	count(c.Id) over() as total_count from Comments c
	where check_post_id = c.PostId
	order by c.CreatedAt desc
	limit limit_count
	offset offset_count;
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
	p_id uuid
) returns uuid as $$
begin
	delete from Comments c
	where p_id = c.Id;
	
	return p_id;
end;
$$ language plpgsql;

create or replace function exists_comment(
	p_id uuid
) returns boolean as $$
begin
	return exists(
		select c.Id from Comments c
		where p_id = c.Id
	);
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

create or replace function get_post_tags(
	p_post_id uuid
) returns table (
	id uuid,
	name varchar(100)
) as $$
begin
	return query
	select t.Id, t.Name from Tags t
	join Posts_Tags pt on pt.TagId = t.Id
	where p_post_id = pt.PostId;
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

create or replace function create_post_tags(
	p_post_id uuid,
	variadic p_tag_names varchar(100)[]
) returns uuid as $$
begin
	insert into Posts_Tags (PostId, TagId)
	select p_post_id, t.Id from Tags t
	where t.Name = any(p_tag_names)
	on conflict (PostId, TagId) do nothing;

	return p_post_id;
end;
$$ language plpgsql;

create or replace function update_post_tags(
	p_post_id uuid,
	variadic p_tag_names varchar(100)[]
) returns uuid as $$
begin
	delete from Posts_Tags
	where PostId = p_post_id;
	
	insert into Posts_Tags (PostId, TagId)
	select p_post_id, t.Id from Tags t
	where t.Name = any(p_tag_names)
	on conflict (PostId, TagId) do nothing;

	return p_post_id;
end;
$$ language plpgsql;

create or replace function exists_tag_by_name(
	check_name varchar(100)
) returns boolean as $$
begin
	return exists(
		select t.Id from Tags t
		where check_name = t.Name
	);
end;
$$ language plpgsql;

create or replace function is_user_like_post(
	post_id uuid,
	user_id uuid
) returns boolean as $$
begin
	return exists(
		select pu.UserId from Posts_Users pu
		where user_id = pu.UserId and post_id = pu.PostId
	);
end;
$$ language plpgsql;

create or replace function get_post_likes_count(
	post_id uuid
) returns integer as $$
begin
	return (
		select count(*) from Posts_Users pu
		where pu.PostId = post_id
	);
end;
$$ language plpgsql;

create or replace function change_like_post_by_user(
	post_id uuid,
	user_id uuid
) returns boolean as $$
begin
	if is_user_like_post(post_id, user_id) then
		delete from Posts_Users pu
		where post_id = pu.PostId and user_id = pu.UserId;

		return false;
	else
		insert into Posts_Users (PostId, UserId)
		values (post_id, user_id);

		return true;
	end if;
end;
$$ language plpgsql;







