#!/bin/bash

# Create certs directory if it doesn't exist
mkdir -p certs
cd certs

# Set variables
PASS=YourSecurePassword
DAYS=365
SUBJECT="/C=US/ST=State/L=City/O=HealthHub/CN=localhost"

# Generate CA key and certificate
openssl genrsa -out ca.key 4096
openssl req -new -x509 -days $DAYS -key ca.key -out ca.crt -subj "$SUBJECT"

# Generate server key and certificate
openssl genrsa -out server.key 4096
openssl req -new -key server.key -out server.csr -subj "$SUBJECT"
openssl x509 -req -days $DAYS -in server.csr -CA ca.crt -CAkey ca.key -CAcreateserial -out server.crt

# Generate client key and certificate
openssl genrsa -out client.key 4096
openssl req -new -key client.key -out client.csr -subj "$SUBJECT"
openssl x509 -req -days $DAYS -in client.csr -CA ca.crt -CAkey ca.key -CAcreateserial -out client.crt

# Create PKCS12 files
openssl pkcs12 -export -out server.pfx -inkey server.key -in server.crt -certfile ca.crt -passout pass:$PASS
openssl pkcs12 -export -out client.pfx -inkey client.key -in client.crt -certfile ca.crt -passout pass:$PASS

# Set permissions
chmod 600 *.key *.pfx
chmod 644 *.crt *.csr

echo "Certificates generated successfully!"
echo "CA certificate: ca.crt"
echo "Server certificate: server.pfx (Password: $PASS)"
echo "Client certificate: client.pfx (Password: $PASS)" 