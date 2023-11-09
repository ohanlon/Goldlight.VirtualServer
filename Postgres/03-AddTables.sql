CREATE TABLE IF NOT EXISTS sv."Organization" (
	"id" uuid NOT NULL UNIQUE,
	"friendlyname" VARCHAR(360) NOT NULL UNIQUE,
	"name" VARCHAR(120) NOT NULL,
	"apikey" varchar(32) NOT NULL UNIQUE,
	"version" bigint NOT NULL DEFAULT '0',
	CONSTRAINT "Organization_pk" PRIMARY KEY ("id")
) WITH (
  OIDS=FALSE
);

CREATE TABLE IF NOT EXISTS sv."User" (
	"id" uuid NOT NULL UNIQUE,
	"userid" varchar(1024) NOT NULL,
	CONSTRAINT "User_pk" PRIMARY KEY ("id")
) WITH (
  OIDS=FALSE
);

CREATE TABLE IF NOT EXISTS sv."Role" (
	"id" uuid NOT NULL UNIQUE,
	"name" varchar(20) NOT NULL UNIQUE,
	CONSTRAINT "Role_pk" PRIMARY KEY ("id")
) WITH (
  OIDS=FALSE
);

CREATE TABLE IF NOT EXISTS sv."OrganizationUser" (
	"organization_id" uuid NOT NULL,
	"user_id" uuid NOT NULL,
	"role_id" uuid NOT NULL
) WITH (
  OIDS=FALSE
);

ALTER TABLE sv."OrganizationUser" DROP CONSTRAINT IF EXISTS "OrganizationUser_fk0",
ADD CONSTRAINT "OrganizationUser_fk0" FOREIGN KEY ("organization_id") REFERENCES sv."Organization"("id") ON DELETE CASCADE;
ALTER TABLE sv."OrganizationUser" DROP CONSTRAINT IF EXISTS "OrganizationUser_fk1",
ADD CONSTRAINT "OrganizationUser_fk1" FOREIGN KEY ("user_id") REFERENCES sv."User"("id") ON DELETE CASCADE;
ALTER TABLE sv."OrganizationUser" DROP CONSTRAINT IF EXISTS "OrganizationUser_fk2",
ADD CONSTRAINT "OrganizationUser_fk2" FOREIGN KEY ("role_id") REFERENCES sv."Role"("id") ON DELETE CASCADE;

CREATE INDEX IF NOT EXISTS "idx-organization-friendlyname"
    ON sv."Organization" USING btree
    (friendlyname COLLATE pg_catalog."default" ASC NULLS LAST)
    TABLESPACE pg_default;

CREATE INDEX IF NOT EXISTS "idx-user-userid"
    ON sv."User" USING btree
    (userid ASC NULLS LAST)
    TABLESPACE pg_default;

CREATE INDEX IF NOT EXISTS "idx-organizationuser-organization-id"
    ON sv."OrganizationUser" USING btree
    (organization_id ASC NULLS LAST)
    TABLESPACE pg_default;

CREATE INDEX IF NOT EXISTS "idx-organizationuser-user-id"
    ON sv."OrganizationUser" USING btree
    (user_id ASC NULLS LAST)
    TABLESPACE pg_default;

CREATE ROLE sv_user WITH
    LOGIN
    NOSUPERUSER
    NOCREATEDB
    NOCREATEROLE
    INHERIT
    NOREPLICATION
    CONNECTION LIMIT -1
    ENCRYPTED PASSWORD 'svpassword';

GRANT ALL ON TABLE sv."Organization" TO postgres;
GRANT DELETE, UPDATE, INSERT, SELECT ON TABLE sv."Organization" TO sv_user;
GRANT ALL ON TABLE sv."Role" TO postgres;
GRANT DELETE, UPDATE, INSERT, SELECT ON TABLE sv."Role" TO sv_user;
GRANT ALL ON TABLE sv."OrganizationUser" TO postgres;
GRANT DELETE, UPDATE, INSERT, SELECT ON TABLE sv."OrganizationUser" TO sv_user;
GRANT ALL ON TABLE sv."User" TO postgres;
GRANT DELETE, UPDATE, INSERT, SELECT ON TABLE sv."User" TO sv_user;

CREATE TABLE IF NOT EXISTS sv."Project"
(
    id uuid NOT NULL,
    name character varying(120) NOT NULL,
    friendlyname character varying(120) NOT NULL,
    description text,
    version bigint NOT NULL,
    PRIMARY KEY (id)
);

CREATE TABLE IF NOT EXISTS sv."OrganizationProjects"
(
    organization_id uuid NOT NULL,
    project_id uuid NOT NULL
);


ALTER TABLE IF EXISTS sv."OrganizationProjects"
    ADD CONSTRAINT fk_organization_project_org FOREIGN KEY (organization_id)
    REFERENCES sv."Organization" (id) MATCH SIMPLE
    ON UPDATE NO ACTION
    ON DELETE NO ACTION
    NOT VALID;


ALTER TABLE IF EXISTS sv."OrganizationProjects"
    ADD CONSTRAINT fk_organization_project_project FOREIGN KEY (project_id)
    REFERENCES sv."Project" (id) MATCH SIMPLE
    ON UPDATE NO ACTION
    ON DELETE NO ACTION
    NOT VALID;

GRANT ALL ON TABLE sv."Project" TO postgres;
GRANT DELETE, UPDATE, INSERT, SELECT ON TABLE sv."Project" TO sv_user;
GRANT ALL ON TABLE sv."OrganizationProjects" TO postgres;
GRANT DELETE, UPDATE, INSERT, SELECT ON TABLE sv."OrganizationProjects" TO sv_user;

CREATE INDEX IF NOT EXISTS "idx-organization-project-project-id"
    ON sv."OrganizationProjects" USING btree
    (project_id ASC NULLS LAST)
    TABLESPACE pg_default;

CREATE INDEX IF NOT EXISTS "idx-organization-project-organization-id"
    ON sv."OrganizationProjects" USING btree
    (organization_id ASC NULLS LAST)
    TABLESPACE pg_default;

    
CREATE INDEX IF NOT EXISTS "idx-project-friendlyname"
    ON sv."Project" USING btree
    (friendlyname ASC NULLS LAST)
    TABLESPACE pg_default;

CREATE INDEX IF NOT EXISTS "idx-project-name"
    ON sv."Project" USING btree
    (name ASC NULLS LAST)
    TABLESPACE pg_default;

    
CREATE INDEX IF NOT EXISTS "idx-project-friendlyname"
    ON sv."Project" USING btree
    (friendlyname ASC NULLS LAST)
    TABLESPACE pg_default;
