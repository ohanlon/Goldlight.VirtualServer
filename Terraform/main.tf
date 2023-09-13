provider "aws" {
  region                      = "eu-west-2"
  access_key                  = "goldlight"
  secret_key                  = "goldlight"
  skip_credentials_validation = true
  skip_metadata_api_check     = true
  skip_requesting_account_id  = true

  endpoints {
    dynamodb = "http://localhost:4566"
  }
}

resource "aws_dynamodb_table" "organizations" {
  name           = "organizations"
  read_capacity  = 20
  write_capacity = 20
  hash_key       = "id"

  attribute {
    name = "id"
    type = "S"
  }
}

resource "aws_dynamodb_table" "projects" {
  name           = "projects"
  read_capacity  = 20
  write_capacity = 20
  hash_key       = "id"
  # range_key      = "organization_id"

  attribute {
    name = "id"
    type = "S"
  }

  attribute {
    name = "organization_id"
    type = "S"
  }

  global_secondary_index {
    name            = "project-organization_id-index"
    hash_key        = "organization_id"
    write_capacity  = 20
    read_capacity   = 20
    projection_type = "ALL"
  }
}
