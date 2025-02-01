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
      commonMajorMinors.push({
        version: common,
        tooltip: versions.join(', ')
      });
    }
  }

  return commonMajorMinors;
}
