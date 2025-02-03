export function getReadableGameVersions(
  versions: string[]
): { version: string; tooltip?: string }[] {
  if (versions.length < 1) {
    return [
      {
        version: 'Unknown',
        tooltip:
          'This mod does not specify any game versions it is compatible with'
      }
    ];
  } else if (versions.length === 1) {
    return versions.map((version) => ({ version }));
  }

  const commonMajorMinors = [] as { version: string; tooltip?: string }[];

  for (const version of versions) {
    const [major, minor] = version.split('.');

    const common = `${major}.${minor}.x`;

    if (!commonMajorMinors.some((v) => v.version === common)) {
      const commonVersions = versions.filter((v) => {
        const [vMajor, vMinor] = v.split('.');
        return vMajor === major && vMinor === minor;
      });

      if (commonVersions.length === 1) {
        commonMajorMinors.push({
          version: version
        });

        continue;
      }

      commonMajorMinors.push({
        version: common,
        tooltip: commonVersions.join(', ')
      });
    }
  }

  return commonMajorMinors;
}

export function getCommonGameVersions(versions: string[]) {
  const mapped = versions
    .map((f) => f.split('.').slice(0, 2).join('.'))
    .map((f) => `${f}.x`);

  return mapped.filter((f, index) => mapped.indexOf(f) === index);
}
