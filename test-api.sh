#!/bin/bash

# Test variables
BASE_URL="https://localhost:8443"
CERT_PATH="./certs/client.pfx"
CERT_PASS="YourSecurePassword"
CA_CERT="./certs/ca.crt"

# Colors for output
GREEN='\033[0;32m'
RED='\033[0;31m'
YELLOW='\033[1;33m'
NC='\033[0m'

# Check if certificates exist
if [ ! -f "$CERT_PATH" ]; then
    echo -e "${RED}Error: Client certificate not found at $CERT_PATH${NC}"
    exit 1
fi

if [ ! -f "$CA_CERT" ]; then
    echo -e "${RED}Error: CA certificate not found at $CA_CERT${NC}"
    exit 1
fi

# Function to make SOAP request
make_soap_request() {
    local endpoint=$1
    local action=$2
    local body=$3
    
    echo -e "${GREEN}Testing $action on $endpoint...${NC}"
    echo -e "${YELLOW}Request:${NC}"
    echo "$body"
    echo
    
    response=$(curl -v -k \
        --http1.1 \
        --cert-type P12 \
        --cert "$CERT_PATH:$CERT_PASS" \
        --cacert "$CA_CERT" \
        -H "Content-Type: text/xml; charset=utf-8" \
        -H "SOAPAction: \"http://healthhub.api.services/$action\"" \
        -w "\nHTTP Status: %{http_code}" \
        -d "$body" \
        "$BASE_URL$endpoint" 2>&1)
    
    status_code=$(echo "$response" | grep "HTTP Status:" | cut -d' ' -f3)
    
    echo -e "${YELLOW}Full Response:${NC}"
    echo "$response"
    echo
    
    if [ -z "$status_code" ] || [ "$status_code" != "200" ]; then
        echo -e "${RED}Error: Request failed${NC}"
        return 1
    fi
}

echo -e "${GREEN}Starting API Tests...${NC}"
echo "----------------------------------------"

# Test EHR Service - GetPatientById
echo -e "${GREEN}Testing EHR Service...${NC}"
make_soap_request "/EhrService.asmx" "GetPatientById" '<?xml version="1.0" encoding="utf-8"?>
<soap:Envelope xmlns:soap="http://schemas.xmlsoap.org/soap/envelope/" xmlns:ser="http://healthhub.api.services">
  <soap:Body>
    <ser:GetPatientById>
      <ser:nationalId>123456789</ser:nationalId>
    </ser:GetPatientById>
  </soap:Body>
</soap:Envelope>'

echo "----------------------------------------"

# Test Insurance Service - GetClaimStatus
echo -e "${GREEN}Testing Insurance Service...${NC}"
make_soap_request "/InsuranceService.asmx" "GetClaimStatus" '<?xml version="1.0" encoding="utf-8"?>
<soap:Envelope xmlns:soap="http://schemas.xmlsoap.org/soap/envelope/" xmlns:ser="http://healthhub.api.services">
  <soap:Body>
    <ser:GetClaimStatus>
      <ser:claimNumber>CLM-20240101-12345678</ser:claimNumber>
    </ser:GetClaimStatus>
  </soap:Body>
</soap:Envelope>'

echo "----------------------------------------"

# Test Government Health Service - ValidatePatientIdentity
echo -e "${GREEN}Testing Government Health Service...${NC}"
make_soap_request "/GovernmentHealthService.asmx" "ValidatePatientIdentity" '<?xml version="1.0" encoding="utf-8"?>
<soap:Envelope xmlns:soap="http://schemas.xmlsoap.org/soap/envelope/" xmlns:ser="http://healthhub.api.services">
  <soap:Body>
    <ser:ValidatePatientIdentity>
      <ser:nationalId>123456789</ser:nationalId>
      <ser:dateOfBirth>1990-01-01T00:00:00</ser:dateOfBirth>
    </ser:ValidatePatientIdentity>
  </soap:Body>
</soap:Envelope>'

echo "----------------------------------------"
echo -e "${GREEN}API Tests Complete${NC}" 