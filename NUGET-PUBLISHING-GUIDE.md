# 📦 NuGet Publishing Guide — ClaudeAI.DotNet

## Step 1: Create a NuGet Account

1. Go to https://www.nuget.org
2. Sign in with your Microsoft account (or create one)
3. Go to **Account → API Keys**
4. Click **Create** → give it a name like `ClaudeAI-DotNet-Publish`
5. Set scope to **Push** and select your package glob: `ClaudeAI.DotNet*`
6. Copy the generated API key — you'll need it

---

## Step 2: Add API Key to GitHub Secrets

1. Go to your repo: https://github.com/shatru123/claude-ai-dotnet-sdk
2. **Settings → Secrets and variables → Actions**
3. Click **New repository secret**
4. Name: `NUGET_API_KEY`
5. Value: paste your NuGet API key
6. Click **Add secret**

---

## Step 3: Publish via GitHub Actions (Recommended)

The CI/CD workflow is already set up in `.github/workflows/ci-cd.yml`.

To trigger a NuGet publish, simply **create a version tag**:

```bash
git tag v1.0.0
git push origin v1.0.0
```

This will:
1. ✅ Build the solution
2. ✅ Run all tests
3. ✅ Pack the NuGet package with the tag version
4. ✅ Push to NuGet.org automatically

---

## Step 4: Manual Publish (Optional)

If you want to publish manually from your machine:

```bash
# Pack the project
dotnet pack src/ClaudeAI.DotNet/ClaudeAI.DotNet.csproj \
  --configuration Release \
  --output ./artifacts \
  /p:Version=1.0.0

# Push to NuGet.org
dotnet nuget push ./artifacts/ClaudeAI.DotNet.1.0.0.nupkg \
  --api-key YOUR_API_KEY \
  --source https://api.nuget.org/v3/index.json
```

---

## Step 5: Verify on NuGet.org

After publishing:
1. Go to https://www.nuget.org/packages/ClaudeAI.DotNet
2. It may take 5–15 minutes to be indexed
3. Test install: `dotnet add package ClaudeAI.DotNet`

---

## Versioning Strategy

Follow **Semantic Versioning (SemVer)**:

| Change | Version Bump | Example |
|--------|-------------|---------|
| Bug fix | Patch | 1.0.0 → 1.0.1 |
| New feature (backward compatible) | Minor | 1.0.0 → 1.1.0 |
| Breaking change | Major | 1.0.0 → 2.0.0 |

Tag format: `v1.0.0`, `v1.1.0`, `v2.0.0`

---

## Release Checklist

- [ ] Update `Version` in `ClaudeAI.DotNet.csproj`
- [ ] Update `CHANGELOG.md` with what changed
- [ ] All tests passing (`dotnet test`)
- [ ] README is up to date
- [ ] Create git tag: `git tag v1.x.x && git push origin v1.x.x`
- [ ] Monitor GitHub Actions for successful publish
- [ ] Verify on nuget.org

---

## Troubleshooting

**Package already exists error:**
Use `--skip-duplicate` flag (already in the workflow).

**401 Unauthorized:**
Check your `NUGET_API_KEY` secret is correct and not expired.

**Package not showing on NuGet:**
NuGet indexing takes up to 15 minutes. Check https://www.nuget.org/packages/ClaudeAI.DotNet.
