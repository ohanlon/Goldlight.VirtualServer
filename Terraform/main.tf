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