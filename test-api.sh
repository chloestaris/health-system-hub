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
        -d "$body" \
        "$BASE_URL$endpoint" 2>&1)
    
    echo -e "${YELLOW}Response:${NC}"
    echo "$response"
    echo
    
    if ! echo "$response" | grep -q "200 OK"; then
        echo -e "${RED}Request failed${NC}"
        return 1
    fi
}

echo -e "${GREEN}Starting API Tests${NC}"
echo "----------------------------------------"

# Test EHR Service
make_soap_request "/EhrService.asmx" "GetPatientById" '<?xml version="1.0" encoding="utf-8"?>
<soap:Envelope xmlns:soap="http://schemas.xmlsoap.org/soap/envelope/" xmlns:ser="http://healthhub.api.services">
  <soap:Body>
    <ser:GetPatientById>
      <ser:nationalId>123456789</ser:nationalId>
    </ser:GetPatientById>
  </soap:Body>
</soap:Envelope>'

# Test Insurance Service
make_soap_request "/InsuranceService.asmx" "GetClaimStatus" '<?xml version="1.0" encoding="utf-8"?>
<soap:Envelope xmlns:soap="http://schemas.xmlsoap.org/soap/envelope/" xmlns:ser="http://healthhub.api.services">
  <soap:Body>
    <ser:GetClaimStatus>
      <ser:claimNumber>CLM123456</ser:claimNumber>
    </ser:GetClaimStatus>
  </soap:Body>
</soap:Envelope>'

# Test Government Health Service
make_soap_request "/GovernmentHealthService.asmx" "ValidatePatientIdentity" '<?xml version="1.0" encoding="utf-8"?>
<soap:Envelope xmlns:soap="http://schemas.xmlsoap.org/soap/envelope/" xmlns:ser="http://healthhub.api.services">
  <soap:Body>
    <ser:ValidatePatientIdentity>
      <ser:nationalId>123456789</ser:nationalId>
      <ser:dateOfBirth>1990-01-01T00:00:00Z</ser:dateOfBirth>
    </ser:ValidatePatientIdentity>
  </soap:Body>
</soap:Envelope>'

echo "----------------------------------------"
echo -e "${GREEN}API Tests Complete${NC}" 