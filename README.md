# Project

This projects uses HMAC (Hash-based Message Authentication Code) to implement JWT (Json web token) authentication from scratch for learning purposes

## How HMAC Works

### HMAC Overview
HMAC (Hash-based Message Authentication Code) is a mechanism that uses a cryptographic hash function (e.g., SHA-256) and a secret key to verify both the data integrity and authenticity of a message. 
The core concept is that HMAC generates a unique hash value from the data and the secret key, which can later be used to verify the integrity of the message by recalculating the hash and comparing it to the original hash.

### How It Works in the Code

1. **Generate a Random Key:**
   A 32-byte random key is generated using `PrivateKeyProvider.GenerateRandomKey(32)`. This key is kept secret and is used to compute the HMAC.

2. **Compute the Hash:**
   The function `HmacProvider.ComputeHash(key, content, HashAlgorithmName.SHA256)` takes three inputs:
   - The secret key.
   - The content (data/message) to sign.
   - The hash algorithm (SHA256 in this case).

   This method produces a hash that is unique to both the content and the key. This hash can be used later to verify that the message has not been tampered with.

3. **Verify the Hash:**
   - You can check if the content is authentic by re-computing the hash using the same key and content. If the recomputed hash matches the original one, the message is valid and hasn't been altered.
   - If you use a different key or alter the content, the resulting hash will not match the original, indicating tampering.
   - If the appropriate hash function is used, this process can't be brute-forced

#### Example from Program.cs
```csharp
Console.WriteLine($"JWT TEST");

string key = PrivateKeyProvider.GenerateRandomKey(32); // 256 bit key
string content = "Your content here";
string signed = HmacProvider.ComputeHash(key, content, HashAlgorithmName.SHA256);

Console.WriteLine($"Key: {key}");
Console.WriteLine($"Content: {content}");
Console.WriteLine($"Hash: {signed}");

Console.WriteLine($"Check if match ? {HmacProvider.ComputeHash(key, content, HashAlgorithmName.SHA256) == signed}");
Console.WriteLine($"Check if match with wrong key ? {HmacProvider.ComputeHash(key + "1", content, HashAlgorithmName.SHA256) == signed}");
Console.WriteLine($"Check if match with diferent payload ? {HmacProvider.ComputeHash(key, content + "1", HashAlgorithmName.SHA256) == signed}");
```

Example output:
```
HMAC TEST
Key: K2JbXWHhc1fljt8ADQ2tAaiuXNgA4rQQUa0L4pH3+BQ=
Content: Your content here
Hash: t4EN84utAPQSX1RszZO+kuWRPrRyuPAwa426joH9KE0=
Check if match ? True
Check if match with wrong key ? False
Check if match with diferent payload ? False
```

## Implementing JWT with HMAC

JWT (JSON Web Token) is a compact, URL-safe token often used in web applications for authentication. A JWT consists of three parts:

**1. Header**: Metadata about the token, such as the type of token and the algorithm used for hashing.

**2. Payload**: The data or claims inside the token (e.g., user information, expiration time).

**3. Signature**: A hash generated using the header, payload, and a secret key. This signature ensures the authenticity and integrity of the token.

The tokens parts are in the following order: 

```base64UrlEncode(header) + "." + base64UrlEncode(payload) + "." + HMAC_SIGNATURE)```

## How JWT Works with HMAC

**1. Create Payload:** The payload contains information you want to encode in the JWT. In my case, the payload includes the user’s UserId, Email, and an expiration date in unix format.

**2. Generate a Key:** A random key is generated to sign the JWT. This key will be used in conjunction with HMAC to create the signature for the JWT.

**3. Create the JWT:** The method ```JwtProvider.CreateToken(key, payload)``` creates a JWT token by encoding the header + "." + payload and signing it using the HMAC algorithm (using SHA-256 in this case). The final token is composed of the encoded header + "." + payload + "." + signature.

**4. Verify the JWT:** To validate the token, the method ```JwtProvider.ValidateToken(token, key)``` is called. It re-computes the signature using the same key and checks if it matches the original signature in the token. If it does, the token is valid. If not, the token might have been tampered with or the wrong key was used.

### Testing

Example from Program.cs

```C#
Console.WriteLine($"JWT TEST");

var payload = new PayloadTest()
{
    UserId = 10,
    Email = "123@gmail.com"
};

payload.AddExpirationDate(DateTime.Now.AddDays(1));

var key = PrivateKeyProvider.GenerateRandomKey(32);
var token = JwtProvider.CreateToken(key, payload);

Console.WriteLine($"Key: {key}");
Console.WriteLine($"Token: {token}");
Console.WriteLine($"Verify token: {JwtProvider.ValidateToken<PayloadTest>(token!, key) != null}");
Console.WriteLine($"Payload: {JsonSerializer.Serialize(JwtProvider.ValidateToken<PayloadTest>(token!, key))}");

Console.WriteLine($"Verify token with wrong key: {JwtProvider.ValidateToken<PayloadTest>(token!, key + "1") != null}");
Console.WriteLine($"Verify token with invalid token: {JwtProvider.ValidateToken<PayloadTest>(token! + "1", key) != null}");
```

Output

```
JWT TEST
Key: PNYyrYhNccWIoE2rOP+ZWa3gNx/+oPTo1u3/KDamWx4=
Token: eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJVc2VySWQiOjEwLCJFbWFpbCI6IjEyM0BnbWFpbC5jb20iLCJFeHAiOjE3MjYxOTk4NzN9.0LFeMLzllOZ2nD6lk6N71vnkTKyIR1uzADvgjtMA9SY
Verify token: True
Payload: {"UserId":10,"Email":"123@gmail.com","Exp":1726199873}
Verify token with wrong key: False
Verify token with invalid token: False
```

Decoding with https://jwt.io/ so we verify that it works

![image](https://github.com/user-attachments/assets/31c53442-1132-48f1-94f0-f13e2ce24791)




