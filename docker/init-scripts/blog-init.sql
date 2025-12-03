create extension if not exists "uuid-ossp";

create table if not exists Posts (
	Id uuid primary key,
	Title varchar(200) not null,
	Content text not null,
	UserId uuid not null,
	CreatedAt timestamptz default now(),
	UpdatedAt timestamptz default null
);

create table if not exists Comments (
	Id uuid primary key,
	PostId uuid references Posts(Id) on delete cascade,
	UserId uuid not null,
	Content text not null,
	CreatedAt timestamptz default now()
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

create table if not exists UserLikes (
	PostId uuid references Posts(Id) on delete cascade,
	UserId uuid not null,
	primary key (PostId, UserId)
);

create index if not exists posts_userid_index on Posts (UserId);
create index if not exists posts_created_at_index on Posts (CreatedAt);
create index if not exists comments_postid_index on Comments (PostId);
create index if not exists comments_created_at_index on Comments (CreatedAt);
create index if not exists tag_name_index on Tags using hash (Name);
create index if not exists post_tags_postid_index on Posts_Tags (PostId);
create index if not exists post_tags_tagid_index on Posts_Tags (TagId);
create index if not exists post_users_postid_index on UserLikes (PostId);
create index if not exists post_users_userid_index on UserLikes (UserId);

create or replace function get_post_by_id(
	p_id uuid
) returns table (
	id uuid,
	title varchar(200),
	content text,
	user_id uuid,
	created_at timestamptz,
	updated_at timestamptz
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
	created_at timestamptz,
	updated_at timestamptz,
	total_count integer
) as $$
begin
	return query
	select distinct p.Id, p.Title, p.Content, p.UserId, p.CreatedAt, p.UpdatedAt,
	count(*) over()::integer as total_count from Posts p
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
	created_at timestamptz,
	updated_at timestamptz,
	total_count integer
) as $$
begin
	return query
	select p.Id, p.Title, p.Content, p.UserId, p.CreatedAt, p.UpdatedAt,
	count(p.Id) over()::integer as total_count from Posts p
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
	p_created_at timestamptz
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
	p_updated_at timestamptz
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
	created_at timestamptz
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
	created_at timestamptz,
	total_count integer
) as $$
begin
	return query
	select c.Id, c.PostId, c.UserId, c.Content, c.CreatedAt,
	count(c.Id) over()::integer as total_count from Comments c
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
	p_created_at timestamptz
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
		select ul.UserId from UserLikes ul
		where user_id = ul.UserId and post_id = ul.PostId
	);
end;
$$ language plpgsql;

create or replace function get_post_likes_count(
	post_id uuid
) returns integer as $$
begin
	return (
		select count(*)::integer from UserLikes ul
		where ul.PostId = post_id
	);
end;
$$ language plpgsql;

create or replace function change_like_post_by_user(
	post_id uuid,
	user_id uuid
) returns boolean as $$
begin
	if is_user_like_post(post_id, user_id) then
		delete from UserLikes ul
		where post_id = ul.PostId and user_id = ul.UserId;

		return false;
	else
		insert into UserLikes (PostId, UserId)
		values (post_id, user_id);

		return true;
	end if;
end;
$$ language plpgsql;

create or replace function get_posts_by_date_range(
	start_date timestamptz,
	end_date timestamptz
) returns table (
	id uuid,
	created_at timestamptz,
	comment_count integer,
	like_count integer
) as $$
begin
	return query
	select p.Id, p.CreatedAt,
	(
		select count(*)::integer from Comments c
		where c.PostId = p.Id
	) as comment_count,
	get_post_likes_count(p.Id) as like_count from Posts p
	where p.CreatedAt > start_date and p.CreatedAt < end_date
	order by p.CreatedAt;
end;
$$ language plpgsql;

create or replace function get_user_activity_posts(
	user_id uuid,
	start_date timestamptz,
	end_date timestamptz
) returns table (
	id uuid,
	created_at timestamptz,
	comment_count integer,
	like_count integer
) as $$
begin
	return query
	select p.Id, p.CreatedAt,
	(
		select count(*)::integer from Comments c
		where c.PostId = p.Id
	) as comment_count,
	get_post_likes_count(p.Id) as like_count from Posts p
	where p.CreatedAt > start_date and p.CreatedAt < end_date and p.UserId = user_id
	order by p.CreatedAt;
end;
$$ language plpgsql;

create or replace function get_user_activity_comments(
	user_id uuid,
	start_date timestamptz,
	end_date timestamptz
) returns table (
	id uuid,
	created_at timestamptz
) as $$
begin
	return query
	select c.Id, c.CreatedAt from Comments c
	where c.CreatedAt > start_date and c.CreatedAt < end_date and c.UserId = user_id
	order by c.CreatedAt;
end;
$$ language plpgsql;

create or replace function get_user_activity_likes(
	user_id uuid,
	start_date timestamptz,
	end_date timestamptz
) returns table (
	id uuid,
	created_at timestamptz
) as $$
begin
	return query
	select p.Id, p.CreatedAt from Posts p
	join UserLikes ul on ul.PostId = p.Id
	where ul.UserId = user_id and p.CreatedAt > start_date and p.CreatedAt < end_date
	order by p.CreatedAt;
end;
$$ language plpgsql;

create or replace function get_tags_statistics(
	take_count integer,
	start_date timestamptz,
	end_date timestamptz
) returns table (
	id uuid,
	name varchar(100),
	posts_count integer
) as $$
begin
	return query
	select t.Id, t.Name, count(distinct p.Id)::integer as posts_count from Tags t
	left join Posts_Tags pt on pt.TagId = t.Id
	left join Posts p on p.Id = pt.PostId
	where p.CreatedAt > start_date and p.CreatedAt < end_date
	group by t.Id, t.Name
	order by posts_count desc
	limit take_count;
end;
$$ language plpgsql;







