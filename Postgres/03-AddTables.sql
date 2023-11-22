CREATE TABLE IF NOT EXISTS sv."Role"
(
    id uuid NOT NULL,
    name character varying(20) COLLATE pg_catalog."default" NOT NULL,
    CONSTRAINT "Role_pk" PRIMARY KEY (id),
    CONSTRAINT "Role_name_key" UNIQUE (name)
)

TABLESPACE pg_default;

ALTER TABLE IF EXISTS sv."Role"
    OWNER to postgres;

REVOKE ALL ON TABLE sv."Role" FROM sv_user;

GRANT ALL ON TABLE sv."Role" TO postgres;

GRANT DELETE, UPDATE, INSERT, SELECT ON TABLE sv."Role" TO sv_user;

-- Table: sv.Organization

-- DROP TABLE IF EXISTS sv."Organization";

CREATE TABLE IF NOT EXISTS sv."Organization"
(
    id uuid NOT NULL,
    friendlyname character varying(360) COLLATE pg_catalog."default" NOT NULL,
    name character varying(120) COLLATE pg_catalog."default" NOT NULL,
    apikey character varying(32) COLLATE pg_catalog."default" NOT NULL,
    version bigint NOT NULL DEFAULT '0'::bigint,
    CONSTRAINT "Organization_pk" PRIMARY KEY (id),
    CONSTRAINT "Organization_apikey_key" UNIQUE (apikey),
    CONSTRAINT "Organization_friendlyname_key" UNIQUE (friendlyname)
)

TABLESPACE pg_default;

ALTER TABLE IF EXISTS sv."Organization"
    OWNER to postgres;

REVOKE ALL ON TABLE sv."Organization" FROM sv_user;

GRANT ALL ON TABLE sv."Organization" TO postgres;

GRANT DELETE, INSERT, SELECT, UPDATE ON TABLE sv."Organization" TO sv_user;
-- Index: idx-organization-api-key

DROP INDEX IF EXISTS sv."idx-organization-api-key";

CREATE INDEX IF NOT EXISTS "idx-organization-api-key"
    ON sv."Organization" USING btree
    (apikey COLLATE pg_catalog."default" ASC NULLS LAST)
    TABLESPACE pg_default;
-- Index: idx-organization-friendlyname

DROP INDEX IF EXISTS sv."idx-organization-friendlyname";

CREATE INDEX IF NOT EXISTS "idx-organization-friendlyname"
    ON sv."Organization" USING btree
    (friendlyname COLLATE pg_catalog."default" ASC NULLS LAST)
    TABLESPACE pg_default;
-- Index: idx-organization-id

DROP INDEX IF EXISTS sv."idx-organization-id";

CREATE INDEX IF NOT EXISTS "idx-organization-id"
    ON sv."Organization" USING btree
    (id ASC NULLS LAST)
    TABLESPACE pg_default;

-- Table: sv.User

-- DROP TABLE IF EXISTS sv."User";

CREATE TABLE IF NOT EXISTS sv."User"
(
    id uuid NOT NULL,
    userid character varying(1024) COLLATE pg_catalog."default" NOT NULL,
    CONSTRAINT "User_pk" PRIMARY KEY (id)
)

TABLESPACE pg_default;

ALTER TABLE IF EXISTS sv."User"
    OWNER to postgres;

REVOKE ALL ON TABLE sv."User" FROM sv_user;

GRANT ALL ON TABLE sv."User" TO postgres;

GRANT DELETE, UPDATE, INSERT, SELECT ON TABLE sv."User" TO sv_user;
-- Index: idx-user-id

DROP INDEX IF EXISTS sv."idx-user-id";

CREATE INDEX IF NOT EXISTS "idx-user-id"
    ON sv."User" USING btree
    (id ASC NULLS LAST)
    TABLESPACE pg_default;
-- Index: idx-user-userid

DROP INDEX IF EXISTS sv."idx-user-userid";

CREATE INDEX IF NOT EXISTS "idx-user-userid"
    ON sv."User" USING btree
    (userid COLLATE pg_catalog."default" ASC NULLS LAST)
    TABLESPACE pg_default;

-- Table: sv.OrganizationUser

-- DROP TABLE IF EXISTS sv."OrganizationUser";

CREATE TABLE IF NOT EXISTS sv."OrganizationUser"
(
    organization_id uuid NOT NULL,
    user_id uuid NOT NULL,
    role_id uuid NOT NULL,
    CONSTRAINT "OrganizationUser_fk0" FOREIGN KEY (organization_id)
        REFERENCES sv."Organization" (id) MATCH SIMPLE
        ON UPDATE NO ACTION
        ON DELETE CASCADE,
    CONSTRAINT "OrganizationUser_fk1" FOREIGN KEY (user_id)
        REFERENCES sv."User" (id) MATCH SIMPLE
        ON UPDATE NO ACTION
        ON DELETE CASCADE,
    CONSTRAINT "OrganizationUser_fk2" FOREIGN KEY (role_id)
        REFERENCES sv."Role" (id) MATCH SIMPLE
        ON UPDATE NO ACTION
        ON DELETE CASCADE
)

TABLESPACE pg_default;

ALTER TABLE IF EXISTS sv."OrganizationUser"
    OWNER to postgres;

REVOKE ALL ON TABLE sv."OrganizationUser" FROM sv_user;

GRANT ALL ON TABLE sv."OrganizationUser" TO postgres;

GRANT DELETE, INSERT, SELECT, UPDATE ON TABLE sv."OrganizationUser" TO sv_user;
-- Index: idx-organizationuser-organization-id

DROP INDEX IF EXISTS sv."idx-organizationuser-organization-id";

CREATE INDEX IF NOT EXISTS "idx-organizationuser-organization-id"
    ON sv."OrganizationUser" USING btree
    (organization_id ASC NULLS LAST)
    TABLESPACE pg_default;
-- Index: idx-organizationuser-user-id

DROP INDEX IF EXISTS sv."idx-organizationuser-user-id";

CREATE INDEX IF NOT EXISTS "idx-organizationuser-user-id"
    ON sv."OrganizationUser" USING btree
    (user_id ASC NULLS LAST)
    TABLESPACE pg_default;

-- Table: sv.Project

-- DROP TABLE IF EXISTS sv."Project";

CREATE TABLE IF NOT EXISTS sv."Project"
(
    id uuid NOT NULL,
    name character varying(120) COLLATE pg_catalog."default" NOT NULL,
    friendlyname character varying(120) COLLATE pg_catalog."default" NOT NULL,
    description text COLLATE pg_catalog."default",
    organization_id uuid NOT NULL,
    version bigint NOT NULL,
    CONSTRAINT "Project_pkey" PRIMARY KEY (id),
    CONSTRAINT fk_idx_project_organization FOREIGN KEY (organization_id)
        REFERENCES sv."Organization" (id) MATCH SIMPLE
        ON UPDATE NO ACTION
        ON DELETE CASCADE
)

TABLESPACE pg_default;

ALTER TABLE IF EXISTS sv."Project"
    OWNER to postgres;

REVOKE ALL ON TABLE sv."Project" FROM sv_user;

GRANT ALL ON TABLE sv."Project" TO postgres;

GRANT DELETE, UPDATE, INSERT, SELECT ON TABLE sv."Project" TO sv_user;
-- Index: idx-project-friendlyname

DROP INDEX IF EXISTS sv."idx-project-friendlyname";

CREATE INDEX IF NOT EXISTS "idx-project-friendlyname"
    ON sv."Project" USING btree
    (friendlyname COLLATE pg_catalog."default" ASC NULLS LAST)
    TABLESPACE pg_default;
-- Index: idx-project-id

DROP INDEX IF EXISTS sv."idx-project-id";

CREATE INDEX IF NOT EXISTS "idx-project-id"
    ON sv."Project" USING btree
    (id ASC NULLS LAST)
    TABLESPACE pg_default;
-- Index: idx-project-name

DROP INDEX IF EXISTS sv."idx-project-name";

CREATE INDEX IF NOT EXISTS "idx-project-name"
    ON sv."Project" USING btree
    (name COLLATE pg_catalog."default" ASC NULLS LAST)
    TABLESPACE pg_default;
-- Index: idx-project-organization

DROP INDEX IF EXISTS sv."idx-project-organization";

CREATE INDEX IF NOT EXISTS "idx-project-organization"
    ON sv."Project" USING btree
    (organization_id ASC NULLS LAST)
    TABLESPACE pg_default;

-- Table: sv.RequestResponse

-- DROP TABLE IF EXISTS sv."RequestResponse";

CREATE TABLE IF NOT EXISTS sv."RequestResponse"
(
    id uuid NOT NULL,
    name character varying(120) COLLATE pg_catalog."default" NOT NULL,
    description character varying(500) COLLATE pg_catalog."default" NOT NULL,
    project_id uuid NOT NULL,
    CONSTRAINT "RequestResponse_pkey" PRIMARY KEY (id),
    CONSTRAINT fk_rrpair_project_id FOREIGN KEY (project_id)
        REFERENCES sv."Project" (id) MATCH SIMPLE
        ON UPDATE NO ACTION
        ON DELETE CASCADE
)

TABLESPACE pg_default;

ALTER TABLE IF EXISTS sv."RequestResponse"
    OWNER to postgres;

REVOKE ALL ON TABLE sv."RequestResponse" FROM sv_user;

GRANT ALL ON TABLE sv."RequestResponse" TO postgres;

GRANT UPDATE, SELECT, DELETE, INSERT ON TABLE sv."RequestResponse" TO sv;

GRANT UPDATE, SELECT, DELETE, INSERT ON TABLE sv."RequestResponse" TO sv_user;
-- Index: idx-requestresponse-id

DROP INDEX IF EXISTS sv."idx-requestresponse-id";

CREATE INDEX IF NOT EXISTS "idx-requestresponse-id"
    ON sv."RequestResponse" USING btree
    (id ASC NULLS LAST)
    TABLESPACE pg_default;
-- Index: idx-requestresponse-project

DROP INDEX IF EXISTS sv."idx-requestresponse-project";

CREATE INDEX IF NOT EXISTS "idx-requestresponse-project"
    ON sv."RequestResponse" USING btree
    (project_id ASC NULLS LAST)
    TABLESPACE pg_default;

-- Table: sv.Request

-- DROP TABLE IF EXISTS sv."Request";

CREATE TABLE IF NOT EXISTS sv."Request"
(
    id uuid NOT NULL,
    content json,
    requestresponse_id uuid NOT NULL,
    CONSTRAINT request_pkey PRIMARY KEY (id),
    CONSTRAINT fk_requestresponse_request_id FOREIGN KEY (requestresponse_id)
        REFERENCES sv."RequestResponse" (id) MATCH SIMPLE
        ON UPDATE NO ACTION
        ON DELETE CASCADE
)

TABLESPACE pg_default;

ALTER TABLE IF EXISTS sv."Request"
    OWNER to postgres;

REVOKE ALL ON TABLE sv."Request" FROM sv_user;

GRANT ALL ON TABLE sv."Request" TO postgres;

GRANT DELETE, UPDATE, INSERT, SELECT ON TABLE sv."Request" TO sv_user;
-- Index: idx-request-rr-id

DROP INDEX IF EXISTS sv."idx-request-rr-id";

CREATE INDEX IF NOT EXISTS "idx-request-rr-id"
    ON sv."Request" USING btree
    (requestresponse_id ASC NULLS LAST)
    TABLESPACE pg_default;

-- Table: sv.RequestHeader

-- DROP TABLE IF EXISTS sv."RequestHeader";

CREATE TABLE IF NOT EXISTS sv."RequestHeader"
(
    id uuid NOT NULL,
    key character varying(120) COLLATE pg_catalog."default" NOT NULL,
    value character varying(1024) COLLATE pg_catalog."default" NOT NULL,
    request_id uuid NOT NULL,
    CONSTRAINT "RequestHeader_pkey" PRIMARY KEY (id),
    CONSTRAINT "fk-responesheader-request" FOREIGN KEY (request_id)
        REFERENCES sv."Request" (id) MATCH SIMPLE
        ON UPDATE NO ACTION
        ON DELETE CASCADE
)

TABLESPACE pg_default;

ALTER TABLE IF EXISTS sv."RequestHeader"
    OWNER to postgres;

REVOKE ALL ON TABLE sv."RequestHeader" FROM sv_user;

GRANT ALL ON TABLE sv."RequestHeader" TO postgres;

GRANT DELETE, UPDATE, INSERT, SELECT ON TABLE sv."RequestHeader" TO sv_user;
-- Index: idx-requestheader-id

DROP INDEX IF EXISTS sv."idx-requestheader-id";

CREATE INDEX IF NOT EXISTS "idx-requestheader-id"
    ON sv."RequestHeader" USING btree
    (id ASC NULLS LAST)
    TABLESPACE pg_default;
-- Index: idx-requestheader-request

DROP INDEX IF EXISTS sv."idx-requestheader-request";

CREATE INDEX IF NOT EXISTS "idx-requestheader-request"
    ON sv."RequestHeader" USING btree
    (request_id ASC NULLS LAST)
    TABLESPACE pg_default;

-- Table: sv.RequestSummary

-- DROP TABLE IF EXISTS sv."RequestSummary";

CREATE TABLE IF NOT EXISTS sv."RequestSummary"
(
    id uuid NOT NULL,
    method character varying(10) COLLATE pg_catalog."default" NOT NULL,
    path character varying(120) COLLATE pg_catalog."default" NOT NULL,
    protocol character varying(64) COLLATE pg_catalog."default",
    request_id uuid NOT NULL,
    CONSTRAINT "RequestSummary_pkey" PRIMARY KEY (id),
    CONSTRAINT fk_idx_requestsummary_response_id FOREIGN KEY (request_id)
        REFERENCES sv."Request" (id) MATCH SIMPLE
        ON UPDATE NO ACTION
        ON DELETE CASCADE
)

TABLESPACE pg_default;

ALTER TABLE IF EXISTS sv."RequestSummary"
    OWNER to postgres;

REVOKE ALL ON TABLE sv."RequestSummary" FROM sv_user;

GRANT ALL ON TABLE sv."RequestSummary" TO postgres;

GRANT UPDATE, SELECT, DELETE, INSERT ON TABLE sv."RequestSummary" TO sv;

GRANT UPDATE, SELECT, DELETE, INSERT ON TABLE sv."RequestSummary" TO sv_user;
-- Index: idx-requestsummary-request

DROP INDEX IF EXISTS sv."idx-requestsummary-request";

CREATE INDEX IF NOT EXISTS "idx-requestsummary-request"
    ON sv."RequestSummary" USING btree
    (request_id ASC NULLS LAST)
    TABLESPACE pg_default;

-- Table: sv.Response

-- DROP TABLE IF EXISTS sv."Response";

CREATE TABLE IF NOT EXISTS sv."Response"
(
    id uuid NOT NULL,
    content json,
    requestresponse_id uuid NOT NULL,
    CONSTRAINT response_pkey PRIMARY KEY (id),
    CONSTRAINT fk_requestresponse_response_id FOREIGN KEY (requestresponse_id)
        REFERENCES sv."RequestResponse" (id) MATCH SIMPLE
        ON UPDATE NO ACTION
        ON DELETE CASCADE
)

TABLESPACE pg_default;

ALTER TABLE IF EXISTS sv."Response"
    OWNER to postgres;

REVOKE ALL ON TABLE sv."Response" FROM sv_user;

GRANT ALL ON TABLE sv."Response" TO postgres;

GRANT DELETE, UPDATE, INSERT, SELECT ON TABLE sv."Response" TO sv_user;
-- Index: idx-response-id

DROP INDEX IF EXISTS sv."idx-response-id";

CREATE INDEX IF NOT EXISTS "idx-response-id"
    ON sv."Response" USING btree
    (id ASC NULLS LAST)
    TABLESPACE pg_default;
-- Index: idx-response-requestresponse-id

DROP INDEX IF EXISTS sv."idx-response-requestresponse-id";

CREATE INDEX IF NOT EXISTS "idx-response-requestresponse-id"
    ON sv."Response" USING btree
    (requestresponse_id ASC NULLS LAST)
    TABLESPACE pg_default;

-- Table: sv.ResponseHeader

-- DROP TABLE IF EXISTS sv."ResponseHeader";

CREATE TABLE IF NOT EXISTS sv."ResponseHeader"
(
    id uuid NOT NULL,
    key character varying(120) COLLATE pg_catalog."default" NOT NULL,
    value character varying(1024) COLLATE pg_catalog."default" NOT NULL,
    response_id uuid NOT NULL,
    CONSTRAINT "ResponseHeader_pkey" PRIMARY KEY (id),
    CONSTRAINT "fk-responesheader-response" FOREIGN KEY (response_id)
        REFERENCES sv."Response" (id) MATCH SIMPLE
        ON UPDATE NO ACTION
        ON DELETE NO ACTION,
    CONSTRAINT "fk-responseheader-response" FOREIGN KEY (response_id)
        REFERENCES sv."Response" (id) MATCH SIMPLE
        ON UPDATE NO ACTION
        ON DELETE CASCADE
)

TABLESPACE pg_default;

ALTER TABLE IF EXISTS sv."ResponseHeader"
    OWNER to postgres;

REVOKE ALL ON TABLE sv."ResponseHeader" FROM sv_user;

GRANT ALL ON TABLE sv."ResponseHeader" TO postgres;

GRANT DELETE, INSERT, SELECT, UPDATE ON TABLE sv."ResponseHeader" TO sv_user;
-- Index: idx-responseheader-id

DROP INDEX IF EXISTS sv."idx-responseheader-id";

CREATE INDEX IF NOT EXISTS "idx-responseheader-id"
    ON sv."ResponseHeader" USING btree
    (id ASC NULLS LAST)
    TABLESPACE pg_default;
-- Index: idx-responseheader-response

DROP INDEX IF EXISTS sv."idx-responseheader-response";

CREATE INDEX IF NOT EXISTS "idx-responseheader-response"
    ON sv."ResponseHeader" USING btree
    (response_id ASC NULLS LAST)
    TABLESPACE pg_default;

-- Table: sv.ResponseSummary

-- DROP TABLE IF EXISTS sv."ResponseSummary";

CREATE TABLE IF NOT EXISTS sv."ResponseSummary"
(
    id uuid NOT NULL,
    protocol character varying(32) COLLATE pg_catalog."default",
    status integer,
    response_id uuid NOT NULL,
    CONSTRAINT "ResponseSummary_pkey" PRIMARY KEY (id),
    CONSTRAINT "fk-responsesummary-response" FOREIGN KEY (response_id)
        REFERENCES sv."Response" (id) MATCH SIMPLE
        ON UPDATE NO ACTION
        ON DELETE CASCADE
)

TABLESPACE pg_default;

ALTER TABLE IF EXISTS sv."ResponseSummary"
    OWNER to postgres;

REVOKE ALL ON TABLE sv."ResponseSummary" FROM sv_user;

GRANT ALL ON TABLE sv."ResponseSummary" TO postgres;

GRANT DELETE, UPDATE, INSERT, SELECT ON TABLE sv."ResponseSummary" TO sv_user;
