# Security Policy

## Supported versions

Only the latest released version of Tahabil receives security fixes.

## Reporting a vulnerability

Email **merqoonline@gmail.com** with a description, reproduction steps and affected version.
Please do not open a public issue for a security defect. Acknowledgement target: 5 business days.

## What is never in this repository

- Licence signing private keys (see `docs/LICENSE_ARCHITECTURE.md`).
- Customer business data, databases, backups or receipts.
- Passwords, tokens, certificates or `.pfx` files.
- Developer machines' machine-binding identifiers.

## Threat model summary

Tahabil is an offline desktop product. The realistic threats are local: another user of the same
Windows account, a copied installation folder, a tampered licence file, a hand-edited database, and
casual licence sharing. Mitigations are documented in `docs/SECURITY_ARCHITECTURE.md`; the licensing
threat model and its honest limits are in `docs/LICENSE_ARCHITECTURE.md`.

## Security practices in the codebase

- Passwords hashed with a memory-hard KDF and per-user salt; never stored or logged in plain text.
- Authorisation enforced in application services, not merely in the UI.
- All SQL parameterised; no string-concatenated SQL.
- Monetary values stored as exact scaled integers; no floating point in financial paths.
- Audit log append-only from the application's perspective.
- Logs scrub password/token/key values before writing.
