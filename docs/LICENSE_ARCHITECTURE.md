# Licence Architecture (offline, lifetime)

## 1. Two applications, one key

| Application | Knows | Can do |
|---|---|---|
| `Tahabil.exe` (main) | **public verification key only** | verify a signed licence, reject an invalid one |
| `TahabilLicenseGenerator.exe` (owner-only) | nothing by default | sign a licence **after** the owner unlocks the private key |

The private key is never compiled into either executable, never committed, never printed to logs and
never included in a release asset.

## 2. Cryptographic choice

**ECDSA P-256 (secp256r1) + SHA-256**, signatures in IEEE-P1363 (64-byte `r‖s`) form.

Why not Ed25519: Ed25519 is an excellent choice and is the modern default for new designs, but on
.NET it would add a third-party dependency (NSec/BouncyCastle) or require a newer API surface than
this product can rely on across the installed base. ECDSA P-256 is:
- built into the base class library (`System.Security.Cryptography.ECDsa`), so there is no dependency
  to audit, patch or license;
- FIPS 186-4 approved and universally reviewed;
- available identically on Windows .NET, in the CI runner and in the generator tool.

The trade-off (P-256 has a higher risk profile than Ed25519 against misuse of nonces if a broken RNG
is used) is accepted because key generation happens on the owner's machine with the OS CSPRNG.

## 3. Licence payload (canonical JSON, UTF-8, keys sorted, no insignificant whitespace)

```json
{
  "algorithm": "ECDSA-P256-SHA256-P1363",
  "business": { "name": "…", "identity": "<sha256 of normalised business identity>" },
  "issueDate": "2026-11-02T00:00:00Z",
  "licenseId": "<guid>",
  "licenseType": "Lifetime",
  "machine": { "binding": "None|Optional|Required", "hash": "<sha256 of machine fingerprint>" },
  "product": "Tahabil",
  "productVersion": "1",
  "serial": 1
}
```
Signature is stored next to the payload in the licence file (`tahabil.license`, JSON):
`{ "payload": { … }, "signature": "base64", "publicKeyId": "…" }`.

## 4. Verification steps in the main application (all offline)

1. File parses and schema version is supported.
2. `product == "Tahabil"`, `algorithm` is the expected one.
3. Signature verifies against the **embedded public key**.
4. Licence is not revoked locally (a revoked-id list ships with the app and is merged with local records).
5. Business identity matches the identity registered on this installation (first activation binds it;
   later edits to the business name are allowed but the bound identity is what is checked).
6. Machine binding policy satisfied (with documented tolerance, see §6).
7. Activation recorded in `license_records` / `activation_records` / `machine_bindings`.
8. Result: lifetime use. **No expiry, no renewal, no internet call, ever.**

## 5. Key custody

- Owner generates the key pair once on an offline machine using the generator's "create key" action.
- Private key is exported **encrypted** (AES-256-GCM with a passphrase, PBKDF2-derived key) into an
  owner-controlled vault (USB stick / encrypted archive), never into this repository.
- The generator requires the passphrase on every launch to unlock the key; it keeps the key in memory
  only and clears it on exit.
- Public key is embedded in the main application source (safe to publish) and pinned in tests.
- Key rotation: a `publicKeyId` field allows the app to carry a *set* of public keys, so a compromised
  or rotated key does not brick existing customers.

## 6. Machine binding and its honest limits

Binding hashes a composite of: Windows `MachineGuid` (registry), system drive volume serial, CPU
architecture and, where present, the SMBIOS UUID. Components are hashed with a product salt, then
combined; the stored value is **not** reversible to hardware identifiers (privacy).

Tolerance policy: `Optional` binding (recommended default) accepts a match on **any 2 of 3**
components, so a NIC swap or RAM upgrade does not lock a shop out. `Required` binding is available for
high-risk deployments but documented as hostile to legitimate hardware repairs.

**Limits, stated plainly:** offline verification cannot be made unbreakable. A determined attacker can
patch the binary. The purpose of this system is to make casual sharing and grey-market copying
unprofitable, to bind a licence to a business identity, and to give the owner a clean, auditable
recovery path — not to defeat a dedicated reverse engineer.

## 7. Recovery workflow

Windows reinstall, disk failure, PC replacement, database restore or an approved business move are
covered by an owner-issued replacement licence:

1. Customer sends their business name and the activation code shown in **About → Licence**.
2. Owner looks up `license_recovery_records`, confirms the identity, issues a replacement licence with
   `serial + 1` and the new machine binding policy (or `None` if the old PC is retired).
3. Old licence is marked superseded. Both the old and new records are kept for audit.
4. Customer imports the new file; the app accepts it because it verifies cryptographically and the
   business identity matches.

No customer is ever permanently locked out for a hardware change.

## 8. Publication policy for the generator

If both executables are attached to the **same public** GitHub Release:
- the generator EXE is public and therefore inspectable — this is acceptable **only** because it holds
  no key material and cannot sign without the owner's passphrase-protected private key;
- if the owner requires the tool to be non-downloadable, it must be distributed from a **private**
  repository or a private channel. A public asset cannot be made "owner-only" by policy alone, and
  this document is the written record of that decision.
