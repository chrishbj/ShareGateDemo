terraform {
  backend "s3" {
    bucket         = "sharegate-demo-tfstate-598886663126-ca-central-1"
    key            = "sharegate-demo/aws/terraform.tfstate"
    region         = "ca-central-1"
    dynamodb_table = "sharegate-demo-tf-locks"
    encrypt        = true
  }
}
