using Lumora.DTOs.Test;
using static Lumora.Configuration.MappingProfiles.MappingConditions;
namespace Lumora.Configuration.MappingProfiles
{
    public class TrainingProgramProfile : AutoMapper.Profile
    {
        public TrainingProgramProfile()
        {
            CreateMap<TrainingProgram, TrainingProgramCreateDto>().ReverseMap();
            CreateMap<TrainingProgram, TrainingProgramUpdateDto>().ForAllMembers(m => m.Condition(PropertyNeedsMapping));
            CreateMap<TrainingProgramUpdateDto, TrainingProgram>().ForAllMembers(m => m.Condition(PropertyNeedsMapping));

            // details
            CreateMap<TrainingProgram, TrainingProgramFullDetailsDto>().ForAllMembers(m => m.Condition(PropertyNeedsMapping));
            CreateMap<ProgramCourse, CourseFullDetailsDto>();
            CreateMap<CourseLesson, LessonFullDetailsDto>();
            CreateMap<LessonAttachment, AttachmentDetailsDto>();
            CreateMap<TestQuestion, TestDetailsDto>();
            CreateMap<TestChoice, ChoiceDetailsDto>();
            CreateMap<TestDetailsDto, TestChoice>();

            CreateMap<CourseWithLessonsCreateDto, ProgramCourse>()
                .ForMember(dest => dest.Lessons, opt => opt.Ignore());
            CreateMap<LessonsWithContentCreateDto, CourseLesson>();
            CreateMap<LessonAttachmentCreateDto, LessonAttachment>();
            CreateMap<TrainingProgram, TrainingProgramFullDetailsDto>()
                .ForMember(dest => dest.Audience, opt => opt.MapFrom(src => src.Audience))
                .ForMember(dest => dest.Requirements, opt => opt.MapFrom(src => src.Requirements))
                .ForMember(dest => dest.Goals, opt => opt.MapFrom(src => src.Goals))
                .ForMember(dest => dest.Topics, opt => opt.MapFrom(src => src.Topics))
                .ForMember(dest => dest.Outcomes, opt => opt.MapFrom(src => src.Outcomes))
                .ForMember(dest => dest.Trainers, opt => opt.MapFrom(src => src.Trainers));

            // for add full program
            CreateMap<TrainingProgram, TrainingProgramCreateDto>().ReverseMap();
            CreateMap<ProgramCourseCreateDto, ProgramCourse>().ForMember(dest => dest.Lessons, opt => opt.MapFrom(src => src.Lessons));
            CreateMap<CourseLessonCreateDto, CourseLesson>().ForMember(dest => dest.LessonAttachments, opt => opt.MapFrom(src => src.LessonAttachments));
            CreateMap<CourseLessonAttachmenCreatetDto, LessonAttachment>();

            CreateMap<LessonUpdateDto, CourseLesson>().ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));

            CreateMap<SingleLessonAttachmentCreateDto, LessonAttachment>();
            CreateMap<LessonAttachmentUpdateDto, LessonAttachment>();
            CreateMap<LessonAttachment, LessonAttachmentDetailsDto>();

            CreateMap<TestUpdateDto, TestQuestion>().ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));
        }
    }
}
