import os
import boto3
import argparse

parser = argparse.ArgumentParser()
parser.add_argument('--path', dest='path', required=True, type=str, help='Path to your data')
parser.add_argument('--key', dest='key', required=True, type=str, help='Object key')
args = parser.parse_args()

# Retrieve the list of existing buckets

access_key = os.environ['AWS_ACCESS_KEY_ID']
secret_key = os.environ['AWS_SECRET_ACCESS_KEY']

s3 = boto3.resource('s3', endpoint_url='https://s3.csh.rit.edu', aws_access_key_id=access_key, aws_secret_access_key=secret_key)
#response = s3.list_buckets()
#print(response)

def upload(file, key):

    bucket = s3.Bucket('devcade-games')

    bucket.upload_file(Filename=file,
                       Key=key)

def list_objects():
    bucket = s3.Bucket('devcade-games')
    for my_bucket_object in bucket.objects.all():
        print(my_bucket_object)

print('uploading...')
upload(args.path, args.key)

print('listing objects')
list_objects()

