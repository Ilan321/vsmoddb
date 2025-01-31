import siteConstants from '~/constants/site.constants';

export default function useTitle(
  title: ComputedRef<string | undefined> | string
) {
  const isString = typeof title === 'string';

  const titleToUse = computed(() => {
    if (isString) {
      if (!title) {
        return siteConstants.title;
      }

      return `${siteConstants.title} - ${title}`;
    }

    if (!title.value) {
      return siteConstants.title;
    }

    return `${siteConstants.title} - ${title.value}`;
  });

  useHead(() => ({
    title: titleToUse.value
  }));
}
