import * as fs from 'fs';

type PackageInfo = {
  name: string;
  version: string;
  license?: string;
  repository?: string;
  homepage?: string;
};

const ALLOWED_LICENSES = [
  '0BSD',
  'Apache-2.0',
  'BSD-2-Clause',
  'BSD-3-Clause',
  'CDDL-1.0',
  'ISC',
  'LGPL-3.0-only',
  'MIT',
  'MPL-2.0',
  'Unlicense',
];

const input = JSON.parse(fs.readFileSync('thirdparty.json', 'utf8'));

const lines = [
  '# Third-Party Notices',
  '',
  'This project includes software developed by third parties.',
  'The following licenses and notices apply.',
  '',
];

const flattened = Object.values(input).flat() as PackageInfo[];
const unknownLicenses = [];

for (const pkg of flattened) {
  const name = pkg.name;
  const version = pkg.version;
  const license = pkg.license || 'UNKNOWN';
  const repo = pkg.repository || pkg.homepage || '';

  lines.push(`## ${name} ${version}`);
  lines.push(`License: ${license}`);
  if (repo) lines.push(repo);
  lines.push('');

  if (!ALLOWED_LICENSES.includes(license)) {
    unknownLicenses.push(`${name} ${version}: ${license}`);
  }
}

fs.writeFileSync('THIRD_PARTY_NOTICES.md', lines.join('\n'));

if (unknownLicenses.length > 0) {
  console.warn('Warning: The following packages have unknown or disallowed licenses:');
  for (const entry of unknownLicenses) {
    console.warn(` - ${entry}`);
  }
}
