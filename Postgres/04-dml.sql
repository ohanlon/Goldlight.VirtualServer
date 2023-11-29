CREATE OR REPLACE VIEW sv.organization_users
 AS
 SELECT org.id,
    org.friendlyname,
    org.name,
    org.apikey,
    org.version,
    usr.userid,
    role.name AS rolename,
	proj.id as project
   FROM sv."Organization" org
     JOIN sv."OrganizationUser" ou ON ou.organization_id = org.id
     JOIN sv."Role" role ON role.id = ou.role_id
	 JOIN sv."Project" proj on proj.organization_id = org.id 
     JOIN sv."User" usr ON usr.id = ou.user_id;

ALTER TABLE sv.organization_users
    OWNER TO postgres;

GRANT ALL ON TABLE sv.organization_users TO postgres;
GRANT SELECT ON TABLE sv.organization_users TO sv_user;

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
    SET name = p_name, description = p_description, version = p_version + 1
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

CREATE OR REPLACE PROCEDURE sv."glsp_SaveRRPair"(
  p_id UUID,
  p_name VARCHAR,
  p_description VARCHAR,
  p_project_id UUID,
  p_request_id UUID,
  p_request_content VARCHAR,
  p_requestsummary_id UUID,
  p_request_method VARCHAR,
  p_request_path VARCHAR,
  p_request_protocol VARCHAR,
  p_response_id UUID,
  p_response_content VARCHAR,
  p_responsesummary_id UUID,
  p_response_protocol VARCHAR,
  p_response_status INTEGER,
  p_version INOUT BIGINT,
  affected_rows OUT INTEGER
)
LANGUAGE 'plpgsql'
AS $$
BEGIN
  IF EXISTS (SELECT 1 FROM sv."RequestResponse" WHERE id = p_id) THEN
    UPDATE sv."RequestResponse"
    SET name = p_name, description = p_description, version = p_version + 1
    WHERE id = p_id AND version = p_version;
    IF FOUND THEN
      affected_rows = 1;
    END IF;

    p_version = p_version + 1;
  ELSE
    p_version = 0;
    INSERT INTO sv."RequestResponse"(id, name, description, project_id, version)
    VALUES (p_id, p_name, p_description, p_project_id, p_version);
    IF FOUND THEN
      affected_rows = 1;
    END IF;
  END IF;
  IF affected_rows = 1 THEN
    CALL sv."glsp_SaveRequest"(p_request_id, p_request_content, p_id, p_requestsummary_id, p_request_method, p_request_path, p_request_protocol);
    CALL sv."glsp_SaveResponse"(p_response_id, p_response_content, p_id, p_responsesummary_id, p_response_protocol, p_response_status);
  END IF;
END;
$$;

CREATE OR REPLACE PROCEDURE sv."glsp_SaveRequest"(
  p_id UUID,
  p_content VARCHAR,
  p_requestresponse_id UUID,
  p_requestsummary_id UUID,
  p_request_method VARCHAR,
  p_request_path VARCHAR,
  p_request_protocol VARCHAR
)
LANGUAGE 'plpgsql'
AS $$
BEGIN
  IF EXISTS (SELECT 1 FROM sv."Request" WHERE id = p_id) THEN
    UPDATE sv."Request"
    SET content = p_content
    WHERE id = p_id;

    UPDATE sv."RequestSummary"
    SET method = p_request_method, "path" = p_request_path, protocol = p_request_protocol
    WHERE id = p_requestsummary_id;
  ELSE
    INSERT INTO sv."Request"(id, content, requestresponse_id)
    VALUES (p_id, p_content, p_requestresponse_id);

    INSERT INTO sv."RequestSummary"(id, method, path, protocol, request_id)
    VALUES (p_requestsummary_id, p_request_method, p_request_path, p_request_protocol, p_id);
  END IF;
END;
$$;

CREATE OR REPLACE PROCEDURE sv."glsp_SaveResponse"(
  p_id UUID,
  p_content VARCHAR,
  p_requestresponse_id UUID,
  p_responsesummary_id UUID,
  p_response_protocol VARCHAR,
  p_response_status INTEGER
)
LANGUAGE 'plpgsql'
AS $$
BEGIN
  IF EXISTS (SELECT 1 FROM sv."Response" WHERE id = p_id) THEN
    UPDATE sv."Response"
    SET content = p_content
    WHERE id = p_id;

    UPDATE sv."ResponseSummary"
    SET protocol = p_response_protocol, status = p_response_status
    WHERE id = p_responsesummary_id;
  ELSE
    INSERT INTO sv."Response"(id, content, requestresponse_id)
    VALUES (p_id, p_content, p_requestresponse_id);

    INSERT INTO sv."ResponseSummary"(id, protocol, status, response_id)
    VALUES(p_responsesummary_id, p_response_protocol, p_response_status, p_id);
  END IF;
END;
$$;
