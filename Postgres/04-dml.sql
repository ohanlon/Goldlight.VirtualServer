CREATE OR REPLACE VIEW sv."organization_users"
AS
SELECT org.id, org.friendlyname, org.name as name, org.apikey, org.version, usr.userid, role.name as rolename
	FROM sv."Organization" org
	INNER JOIN sv."OrganizationUser" ou
	ON ou.organization_id = org.id
	INNER JOIN sv."Role" role
	ON role.id = ou.role_id
	INNER JOIN sv."User" usr
	ON usr.id = ou.user_id;

GRANT SELECT ON TABLE sv."organization_users" TO sv_user;

CREATE OR REPLACE PROCEDURE sv."glsp_SaveOrganization"(
    p_id UUID,
    p_friendlyname VARCHAR,
    p_name VARCHAR,
    p_apikey VARCHAR,
	  p_email VARCHAR,
    p_version INOUT BIGINT,
	affected_rows OUT INTEGER
)
LANGUAGE plpgsql
AS $$
BEGIN
    affected_rows = 0;
    IF EXISTS (SELECT 1 FROM sv."Organization" WHERE id = p_id) THEN
        -- Update the existing record
        
        UPDATE sv."Organization"
        SET name = p_name,
			      apikey = p_apikey,
            version = p_version + 1
        WHERE id = p_id AND version = p_version;
        IF FOUND THEN
  	    	affected_rows = 1;
        END IF;

        p_version = p_version + 1;
    ELSE
        INSERT INTO sv."Organization" (id, friendlyname, name, apikey, version)
        VALUES (p_id, p_friendlyname, p_name, p_apikey, p_version); 
		    IF FOUND THEN
			    affected_rows = 1;
		    END IF;
		    CALL sv."glsp_SaveUserRoles"(p_id, p_email, 'PRIMARY OWNER');

    END IF;
END;
$$;

CREATE OR REPLACE PROCEDURE sv."glsp_InsertRoles"(
	)
LANGUAGE 'plpgsql'
AS $$
BEGIN
    INSERT INTO sv."Role" (id, name)
    SELECT gen_random_uuid(), 'PRIMARY OWNER'
    WHERE NOT EXISTS (SELECT 1 FROM sv."Role" WHERE name = 'PRIMARY OWNER');
    INSERT INTO sv."Role" (id, name)
    SELECT gen_random_uuid(), 'OWNER'
    WHERE NOT EXISTS (SELECT 1 FROM sv."Role" WHERE name = 'OWNER');
    INSERT INTO sv."Role" (id, name)
    SELECT gen_random_uuid(), 'EDITOR'
    WHERE NOT EXISTS (SELECT 1 FROM sv."Role" WHERE name = 'EDITOR');
    INSERT INTO sv."Role" (id, name)
    SELECT gen_random_uuid(), 'SUBSCRIBER'
    WHERE NOT EXISTS (SELECT 1 FROM sv."Role" WHERE name = 'SUBSCRIBER');
END;
$$;

CREATE OR REPLACE PROCEDURE sv."glsp_AddUser"(p_email VARCHAR)
LANGUAGE 'plpgsql'
AS $$
BEGIN
    INSERT INTO sv."User" (id, userid) SELECT gen_random_uuid(), p_email
    WHERE NOT EXISTS (SELECT 1 FROM sv."User" where userid = p_email);
END;
$$;

CREATE OR REPLACE PROCEDURE sv."glsp_SaveUserRoles"(
  p_orgId UUID,
  p_email VARCHAR,
  p_role VARCHAR
)
LANGUAGE 'plpgsql'
AS $$
DECLARE
  roleId UUID;
  userIdentity UUID;
BEGIN
    CALL sv."glsp_AddUser"(p_email);
    SELECT id INTO userIdentity FROM sv."User" where userid = p_email;
    SELECT id INTO roleId FROM sv."Role" where name = p_role;
    INSERT INTO sv."OrganizationUser" ("organization_id", "user_id", "role_id") VALUES (p_orgId, userIdentity, roleId);
END;
$$;

CREATE OR REPLACE PROCEDURE sv."glsp_SaveProject"(
  p_id UUID,
  p_name VARCHAR,
  p_friendlyname VARCHAR,
  p_description TEXT,
  p_organization UUID,
  p_version INOUT BIGINT,
  affected_rows OUT INTEGER
)
LANGUAGE 'plpgsql'
AS $$
BEGIN
  IF EXISTS (SELECT 1 FROM sv."Project" WHERE id = p_id) THEN
    UPDATE sv."Project"
    SET name = p_name, description = p_description
    WHERE id = p_id AND version = p_version;
    IF FOUND THEN
      affected_rows = 1;
    END IF;

    p_version = p_version + 1;
  ELSE
    p_version = 0;
    INSERT INTO sv."Project"(id, name, friendlyname, description, organization_id, version) VALUES 
    (p_id, p_name, p_friendlyname, p_description, p_organization, p_version);
    IF FOUND THEN
      affected_rows = 1;
    END IF;
  END IF;
END;
$$;

CREATE TYPE sv.header_info AS
(
	id uuid,
	key character varying(120),
	value character varying(120)
);

ALTER TYPE sv.header_info
    OWNER TO postgres;

