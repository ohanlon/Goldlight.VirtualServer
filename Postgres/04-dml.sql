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

CREATE OR REPLACE PROCEDURE sv."glsp_InsertOrganization"(
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
		INSERT INTO sv."User"(id, userid) VALUES (gen_random_uuid(), p_email);
		IF FOUND THEN
  	    	affected_rows = 1;
		END IF;
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
