# HealthHub Integration API

A secure healthcare integration hub API that connects hospitals with legacy EHR systems, insurance providers, and government health agencies using SOAP services and mutual TLS authentication.

## Features

- SOAP-based API services for:
  - Electronic Health Records (EHR)
  - Insurance Claims
  - Government Health Reporting
- Mutual TLS (mTLS) authentication
- SQL Server database integration
- Docker containerization
- HIPAA-compliant security measures

## Prerequisites

- Docker
- Docker Compose
- OpenSSL (for certificate generation)

## Getting Started

1. Clone the repository:
```bash
git clone [repository-url]
cd health-system-hub
```

2. Generate SSL certificates for development:
```bash
chmod +x generate-certs.sh
./generate-certs.sh
```

3. Start the services:
```bash
docker-compose up --build
```

The API will be available at `https://localhost:8443`

## Security Features

- Mutual TLS (mTLS) authentication
- HTTPS-only communication
- Secure headers implementation
- SQL injection protection through Entity Framework
- Audit logging
- Input validation and sanitization

## API Services

### EHR Service
- Get patient information
- Manage health records
- Update patient data

### Insurance Service
- Submit insurance claims
- Check claim status
- Retrieve patient claims history

### Government Health Service
- Report health records to government agencies
- Validate patient identity
- Report infectious diseases
- Access patient health history

## Development Notes

- The default password for the SSL certificates is "YourSecurePassword"
- The SQL Server SA password is "YourStrong!PassW0rd"
- In production, replace all default passwords with secure values
- Configure proper certificate validation in production

## Testing

For testing SOAP services, you can use tools like:
- SoapUI
- Postman (with SOAP support)
- Your preferred SOAP client

Example SOAP request for getting patient information:
```xml
<soap:Envelope xmlns:soap="http://www.w3.org/2003/05/soap-envelope">
  <soap:Header/>
  <soap:Body>
    <GetPatientByIdAsync xmlns="http://tempuri.org/">
      <nationalId>123456789</nationalId>
    </GetPatientByIdAsync>
  </soap:Body>
</soap:Envelope>
```

## Production Deployment

For production deployment:
1. Replace self-signed certificates with proper CA-signed certificates
2. Update all default passwords and connection strings
3. Configure proper firewall rules
4. Enable additional security measures as required by HIPAA
5. Set up proper monitoring and logging
6. Configure backup and disaster recovery

## License

[Your License] 